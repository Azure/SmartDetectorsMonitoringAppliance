//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Common.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Common.Extensions;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;
    using Microsoft.Rest.Azure.OData;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// Implementation of the <see cref="IAzureResourceManagerClient"/> interface
    /// </summary>
    public class AzureResourceManagerClient : IAzureResourceManagerClient
    {
        /// <summary>
        /// The maximal number of allowed resources to enumerate
        /// </summary>
        private const int MaxResourcesToEnumerate = 100;

        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "ARM";

        private const string SubscriptionRegexPattern = "/subscriptions/(?<subscriptionId>[^/]*)";
        private const string ResourceGroupRegexPattern = SubscriptionRegexPattern + "/resourceGroups/(?<resourceGroupName>[^/]*)";
        private const string ResourceRegexPattern = ResourceGroupRegexPattern + "/providers/(?<resourceProviderAndType>.*)/(?<resourceName>[^/]*)";

        private static readonly ConcurrentDictionary<string, ProviderInner> ProvidersCache = new ConcurrentDictionary<string, ProviderInner>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// A dictionary, mapping <see cref="ResourceType"/> enumeration values to matching ARM string
        /// </summary>
        private static readonly Dictionary<ResourceType, string> MapResourceTypeToString = new Dictionary<ResourceType, string>()
        {
            [ResourceType.VirtualMachine] = "Microsoft.Compute/virtualMachines",
            [ResourceType.ApplicationInsights] = "Microsoft.Insights/components",
            [ResourceType.LogAnalytics] = "Microsoft.OperationalInsights/workspaces"
        };

        /// <summary>
        /// A dictionary, mapping ARM strings to their matching <see cref="ResourceType"/> enumeration values
        /// </summary>
        private static readonly Dictionary<string, ResourceType> MapStringToResourceType = MapResourceTypeToString.ToDictionary(x => x.Value, x => x.Key, StringComparer.CurrentCultureIgnoreCase);

        private readonly ServiceClientCredentials credentials;
        private readonly ITracer tracer;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class 
        /// </summary>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="tracer">The tracer</param>
        public AzureResourceManagerClient(ICredentialsFactory credentialsFactory, ITracer tracer)
        {
            this.credentials = credentialsFactory.Create("https://management.azure.com/");
            this.tracer = tracer;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Gets the resource ID that represents the resource identified by the specified <see cref="ResourceIdentifier"/> structure.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceIdentifier">The <see cref="ResourceIdentifier"/> structure.</param>
        /// <returns>The resource ID.</returns>
        public string GetResourceId(ResourceIdentifier resourceIdentifier)
        {
            // Find the regex pattern based on the type
            string pattern;
            string resourceProviderAndType = string.Empty;
            switch (resourceIdentifier.ResourceType)
            {
                case ResourceType.Subscription:
                    pattern = SubscriptionRegexPattern;
                    break;
                case ResourceType.ResourceGroup:
                    pattern = ResourceGroupRegexPattern;
                    break;
                default:
                    pattern = ResourceRegexPattern;
                    if (!MapResourceTypeToString.TryGetValue(resourceIdentifier.ResourceType, out resourceProviderAndType))
                    {
                        throw new ArgumentException($"Resource type {resourceIdentifier.ResourceType} is not supported");
                    }

                    break;
            }

            // Replace the pattern components based on the resource identifier properties
            pattern = pattern.Replace("(?<subscriptionId>[^/]*)", resourceIdentifier.SubscriptionId);
            if (resourceIdentifier.ResourceType != ResourceType.Subscription)
            {
                pattern = pattern.Replace("(?<resourceGroupName>[^/]*)", resourceIdentifier.ResourceGroupName);
                if (resourceIdentifier.ResourceType != ResourceType.ResourceGroup)
                {
                    pattern = pattern.Replace("(?<resourceProviderAndType>.*)", resourceProviderAndType);
                    pattern = pattern.Replace("(?<resourceName>[^/]*)", resourceIdentifier.ResourceName);
                }
            }

            return pattern;
        }

        /// <summary>
        /// Gets the <see cref="ResourceIdentifier"/> structure that represents the resource identified by the specified resource ID.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceId">The resource ID</param>
        /// <returns>The <see cref="ResourceIdentifier"/> structure.</returns>
        public ResourceIdentifier GetResourceIdentifier(string resourceId)
        {
            // Match resource pattern
            Match m = Regex.Match(resourceId, ResourceRegexPattern);
            if (m.Success)
            {
                // Verify that the resource is of a supported type
                string resourceProviderAndType = m.Groups["resourceProviderAndType"].Value;
                if (!MapStringToResourceType.TryGetValue(resourceProviderAndType, out ResourceType resourceType))
                {
                    throw new ArgumentException($"Resource type {resourceType} is not supported");
                }

                return ResourceIdentifier.Create(resourceType, m.Groups["subscriptionId"].Value, m.Groups["resourceGroupName"].Value, m.Groups["resourceName"].Value);
            }

            // Match resource group pattern
            m = Regex.Match(resourceId, ResourceGroupRegexPattern);
            if (m.Success)
            {
                return ResourceIdentifier.Create(m.Groups["subscriptionId"].Value, m.Groups["resourceGroupName"].Value);
            }

            // Match subscription pattern
            m = Regex.Match(resourceId, SubscriptionRegexPattern);
            if (m.Success)
            {
                return ResourceIdentifier.Create(m.Groups["subscriptionId"].Value);
            }

            throw new ArgumentException($"Invalid resource ID provided: {resourceId}");
        }

        /// <summary>
        /// Enumerates all the resource groups in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource groups.</returns>
        public async Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            Task<IPage<ResourceGroupInner>> FirstPage() => resourceManagementClient.ResourceGroups.ListAsync(cancellationToken: cancellationToken);
            Task<IPage<ResourceGroupInner>> NextPage(string nextPageLink) => resourceManagementClient.ResourceGroups.ListNextAsync(nextPageLink, cancellationToken);
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage, "resource groups in subscription")))
                .Select(resourceGroup => this.GetResourceIdentifier(resourceGroup.Id))
                .ToList();
        }

        /// <summary>
        /// Enumerates all the resources of the specified types in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        public async Task<IList<ResourceIdentifier>> GetAllResourcesInSubscriptionAsync(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            ODataQuery<GenericResourceFilterInner> query = this.GetResourcesByTypeQuery(resourceTypes);
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.Resources.ListAsync(query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.Resources.ListNextAsync(nextPageLink, cancellationToken);
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage, "resources in subscription")))
                .Select(resource => this.GetResourceIdentifier(resource.Id))
                .ToList();
        }

        /// <summary>
        /// Enumerates all the resources in the specified resource group.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceGroupName">The resource group name.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        public async Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroupAsync(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            ODataQuery<GenericResourceFilterInner> query = this.GetResourcesByTypeQuery(resourceTypes);
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.ResourceGroups.ListResourcesAsync(resourceGroupName, query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.ResourceGroups.ListResourcesNextAsync(nextPageLink, cancellationToken);
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage, "resources in resource group")))
                .Select(resource => this.GetResourceIdentifier(resource.Id))
                .ToList();
        }

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        public async Task<IList<string>> GetAllSubscriptionIdsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var subscriptions = await this.RunAndTrack(() => this.GetSubscriptionClient().Subscriptions.ListAsync(cancellationToken));
            return subscriptions.Select(subscription => subscription.SubscriptionId).ToList();
        }

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        public async Task<IList<SubscriptionInner>> GetAllSubscriptionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await this.GetSubscriptionClient().Subscriptions.ListAsync(cancellationToken)).ToList();
        }

        /// <summary>
        /// Returns the resource properties, as a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource properties.</returns>
        public async Task<JObject> GetResourcePropertiesAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken)
        {
            var client = this.GetResourceManagementClient(resourceIdentifier.SubscriptionId);

            // Get the resource type string
            if (!MapResourceTypeToString.TryGetValue(resourceIdentifier.ResourceType, out string resourceTypeString))
            {
                throw new ArgumentException($"Resource type {resourceIdentifier.ResourceType} is not supported for the GetResourceProperties method");
            }

            // Extract the provider and resource type
            ParseResourceTypeString(resourceTypeString, out string provider, out string type);

            // Get the API version that should be used for this resource type
            string apiVersion = await this.GetApiVersionAsync(client, provider, type, cancellationToken);

            // Get the resource
            var resource = await this.RunAndTrack(() => client.Resources.GetAsync(
                resourceIdentifier.ResourceGroupName,
                provider,
                string.Empty,
                type,
                resourceIdentifier.ResourceName,
                apiVersion,
                cancellationToken));

            // Get the resource properties as a JObject
            return resource.Properties as JObject;
        }

        /// <summary>
        /// Returns the application insights app ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The application insights resource identifier.
        /// The value of the <see cref="ResourceType"/> property must be equal to <see cref="ResourceType.ApplicationInsights"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the app ID.</returns>
        public async Task<string> GetApplicationInsightsAppIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken)
        {
            if (resourceIdentifier.ResourceType != ResourceType.ApplicationInsights)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceIdentifier.ResourceType), resourceIdentifier.ResourceType, "The resource type must be ApplicationInsights");
            }

            // Extract the AppId from the resource properties
            JObject properties = await this.GetResourcePropertiesAsync(resourceIdentifier, cancellationToken);
            string applicationId = properties["AppId"].ToObject<string>();
            if (applicationId == null)
            {
                throw new ArgumentException($"No application ID found for resource {resourceIdentifier.ResourceName}");
            }

            return applicationId;
        }

        /// <summary>
        /// Returns the log analytics workspace ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The log analytics resource identifier.
        /// The value of the <see cref="ResourceType"/> property must be equal to <see cref="ResourceType.LogAnalytics"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the workspace ID.</returns>
        public async Task<string> GetLogAnalyticsWorkspaceIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken)
        {
            if (resourceIdentifier.ResourceType != ResourceType.LogAnalytics)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceIdentifier.ResourceType), resourceIdentifier.ResourceType, "The resource type must be ApplicationInsights");
            }

            // Extract the workspace ID from the resource properties
            JObject properties = await this.GetResourcePropertiesAsync(resourceIdentifier, cancellationToken);
            string workspaceId = properties["customerId"].ToObject<string>();
            if (workspaceId == null)
            {
                throw new ArgumentException($"No workspace ID found for resource {resourceIdentifier.ResourceName}");
            }

            return workspaceId;
        }

        /// <summary>
        /// Separates the resource type string to a provider and resource type components.
        /// </summary>
        /// <param name="resourceTypeString">The resource type string.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="type">The resource type.</param>
        private static void ParseResourceTypeString(string resourceTypeString, out string provider, out string type)
        {
            int slashPosition = resourceTypeString.IndexOf("/", StringComparison.CurrentCulture);
            provider = resourceTypeString.Substring(0, slashPosition);
            type = resourceTypeString.Substring(slashPosition + 1);
        }

        /// <summary>
        /// Gets the provider information, either from cache or using the ARM client.
        /// </summary>
        /// <param name="client">The ARM client.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the provider information.</returns>
        private async Task<ProviderInner> GetProviderInformationAsync(ResourceManagementClient client, string provider, CancellationToken cancellationToken)
        {
            return ProvidersCache.GetOrAdd(
                provider,
                await this.RunAndTrack(() => client.Providers.GetAsync(provider, null, cancellationToken)));
        }

        /// <summary>
        /// Returns the API version to use for getting data for resources of the specified provider and type.
        /// This method always returns the latest API version.
        /// </summary>
        /// <param name="client">The ARM client.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="type">The resource type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the API version.</returns>
        private async Task<string> GetApiVersionAsync(ResourceManagementClient client, string provider, string type, CancellationToken cancellationToken)
        {
            ProviderInner providerInformation = await this.GetProviderInformationAsync(client, provider, cancellationToken);
            ProviderResourceType providerResourceType = providerInformation.ResourceTypes.FirstOrDefault(resourceType => resourceType.ResourceType.Equals(type, StringComparison.CurrentCultureIgnoreCase));
            if (providerResourceType == null)
            {
                throw new ArgumentException($"Provider {provider} does not support type {type}");
            }

            return providerResourceType.ApiVersions.Max();
        }

        /// <summary>
        /// Creates a new subscription client
        /// </summary>
        /// <returns>The subscription client</returns>
        private SubscriptionClient GetSubscriptionClient()
        {
            return new SubscriptionClient(this.credentials);
        }

        /// <summary>
        /// Creates a new resource manager client and initializes it with the specified subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <returns>The resource manager client</returns>
        private ResourceManagementClient GetResourceManagementClient(string subscriptionId)
        {
            return new ResourceManagementClient(this.credentials) { SubscriptionId = subscriptionId };
        }

        /// <summary>
        /// Enumerate all results, using the specified paging functions.
        /// </summary>
        /// <typeparam name="T">The type of item returned in the paged results</typeparam>
        /// <exception cref="TooManyResourcesException">Thrown when too many items are found</exception>
        /// <param name="firstPage">A function that returns the first results page</param>
        /// <param name="nextPage">A function that returns the next results page, given the next page link</param>
        /// <param name="enumerationDescription">The enumeration description, to include in the exception error message</param>
        /// <returns>The list of items, read from the paged results</returns>
        private async Task<List<T>> ReadAllPages<T>(Func<Task<IPage<T>>> firstPage, Func<string, Task<IPage<T>>> nextPage, string enumerationDescription)
        {
            List<T> items = new List<T>();
            IPage<T> currentPage = await firstPage();
            while (currentPage != null)
            {
                // Read all items from the current page
                int prevCount = items.Count;
                items.AddRange(currentPage);
                int currentPageCount = items.Count - prevCount;

                // Check limit
                if (items.Count >= MaxResourcesToEnumerate)
                {
                    throw new TooManyResourcesException($"Could not enumerate {enumerationDescription} - over {MaxResourcesToEnumerate} items found");
                }

                // If this is the last page, we are done
                if (currentPageCount == 0 || string.IsNullOrEmpty(currentPage.NextPageLink))
                {
                    break;
                }

                // Get the next page
                currentPage = await nextPage(currentPage.NextPageLink);
            }

            return items;
        }

        /// <summary>
        /// Runs the async ARM operation, tracking dependency and applying retry policy
        /// </summary>
        /// <typeparam name="T">The operation result type</typeparam>
        /// <param name="dependencyCall">The operation</param>
        /// <param name="commandName">The command name</param>
        /// <returns>The operation result</returns>
        private Task<T> RunAndTrack<T>(Func<Task<T>> dependencyCall, [CallerMemberName] string commandName = null)
        {
            return this.retryPolicy.RunAndTrackDependencyAsync(this.tracer, DependencyName, commandName, dependencyCall);
        }

        /// <summary>
        /// Builds an OData query object, that filters the resources by the specified resource types.
        /// </summary>
        /// <param name="resourceTypes">The resource types to filter by.</param>
        /// <returns>The OData query object.</returns>
        private ODataQuery<GenericResourceFilterInner> GetResourcesByTypeQuery(IEnumerable<ResourceType> resourceTypes)
        {
            // Convert the resource types to ARM strings
            List<string> resourceTypesStrings = new List<string>();
            foreach (ResourceType resourceType in resourceTypes)
            {
                if (!MapResourceTypeToString.TryGetValue(resourceType, out string resourceTypeString))
                {
                    throw new ArgumentException($"Resource type {resourceType} is not supported");
                }

                resourceTypesStrings.Add(resourceTypeString);
            }

            // Concatenate all the equality conditions with "or"
            string queryString = string.Join(" or ", resourceTypesStrings.Select(resourceType => "resourceType eq '" + resourceType.Replace("'", "''") + "'"));
            return new ODataQuery<GenericResourceFilterInner>(queryString);
        }
    }
}