//-----------------------------------------------------------------------
// <copyright file="ExtendedAzureResourceManagerClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Rest.Azure;
    using Microsoft.Rest.Azure.OData;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// Implementation of the <see cref="IExtendedAzureResourceManagerClient"/> interface
    /// </summary>
    public class ExtendedAzureResourceManagerClient : IExtendedAzureResourceManagerClient
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "ARM";

        /// <summary>
        /// The HTTP request timeout for ARM calls, in minutes
        /// </summary>
        private const int HttpRequestTimeoutInMinutes = 5;

        private static readonly ConcurrentDictionary<string, ProviderInner> ProvidersCache = new ConcurrentDictionary<string, ProviderInner>(StringComparer.CurrentCultureIgnoreCase);

        private readonly AzureCredentials credentials;
        private readonly ITracer tracer;
        private readonly Policy retryPolicy;
        private readonly Policy<HttpResponseMessage> httpRetryPolicy;
        private readonly Uri baseUri;
        private readonly IHttpClientWrapper httpClientWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedAzureResourceManagerClient"/> class
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="tracer">The tracer</param>
        public ExtendedAzureResourceManagerClient(IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, ITracer tracer)
        {
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            this.baseUri = new Uri(ConfigurationManager.AppSettings["ResourceManagerBaseUri"] ?? "https://management.azure.com/");
            this.credentials = credentialsFactory.CreateAzureCredentials(ConfigurationManager.AppSettings["ResourceManagerCredentialsResource"] ?? "https://management.azure.com/");
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.tracer = tracer;
            this.retryPolicy = Policy
                .Handle<CloudException>(ex => ex.Request != null && (ex.Response.StatusCode >= HttpStatusCode.InternalServerError || ex.Response.StatusCode == HttpStatusCode.RequestTimeout))
                .WaitAndRetryAsync(
                    3,
                    (i) => TimeSpan.FromSeconds(Math.Pow(2, i)),
                    (exception, span, context) => tracer.TraceError($"Failed accessing DependencyName on {exception.Message}, retry {Math.Log(span.Seconds, 2)} out of 3"));
            this.httpRetryPolicy = PolicyExtensions.CreateTransientHttpErrorPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Gets a mapping between resource provider and default API version
        /// </summary>
        private static IReadOnlyDictionary<string, string> ProviderToDefaultApiVersionMapping =>
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    { "Microsoft.Insights", "2015-05-01" },
                    { "Microsoft.OperationalInsights", "2015-03-20" }
                });

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
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage)))
                .Select(resourceGroup => ResourceIdentifier.CreateFromResourceId(resourceGroup.Id))
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
            ODataQuery<GenericResourceFilterInner> query = GetResourcesByTypeQuery(resourceTypes);
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.Resources.ListAsync(query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.Resources.ListNextAsync(nextPageLink, cancellationToken);
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage)))
                .Select(resource => ResourceIdentifier.CreateFromResourceId(resource.Id))
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
            ODataQuery<GenericResourceFilterInner> query = GetResourcesByTypeQuery(resourceTypes);
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.Resources.ListByResourceGroupAsync(resourceGroupName, query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.Resources.ListByResourceGroupNextAsync(nextPageLink, cancellationToken);
            return (await this.RunAndTrack(() => this.ReadAllPages(FirstPage, NextPage)))
                .Select(resource => ResourceIdentifier.CreateFromResourceId(resource.Id))
                .ToList();
        }

        /// <summary>
        /// Enumerates all the accessible subscriptions IDs.
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
        public async Task<IList<AzureSubscription>> GetAllSubscriptionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await this.RunAndTrack(() => this.GetSubscriptionClient().Subscriptions.ListAsync(cancellationToken)))
                .Select(sub => new AzureSubscription(sub.SubscriptionId, sub.DisplayName)).ToList();
        }

        /// <summary>
        /// Retrieves the properties of the specified resource from ARM, using default API version stated in <see cref="ProviderToDefaultApiVersionMapping"/>.
        /// If no default API version is present - the latest, non preview, version is used.
        /// The returned object contains the resource properties. Every ARM resource has different properties with
        /// different schema.
        /// For schema details for the properties of a specific ARM resource type:
        /// <code>https://docs.microsoft.com/azure/templates/{provider-namespace}/{resource-type}</code>
        /// For example, <a href="https://docs.microsoft.com/en-us/azure/templates/Microsoft.Compute/virtualMachines">this link</a>
        /// provides the schema for virtual machine properties.
        /// </summary>
        /// <param name="resource">The specific resource for which to retrieve the properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, and returning the resource properties.</returns>
        public Task<ResourceProperties> GetResourcePropertiesAsync(ResourceIdentifier resource, CancellationToken cancellationToken)
        {
            return this.GetResourcePropertiesAsync(resource, null, cancellationToken);
        }

        /// <summary>
        /// Retrieves all the properties of the specified resource from ARM, using the specified ARM API version.
        /// The returned object contains the resource properties. Every ARM resource has different properties with
        /// different schema.
        /// For schema details for the properties of a specific ARM resource type:
        /// <code>https://docs.microsoft.com/azure/templates/{provider-namespace}/{resource-type}</code>
        /// For example, <a href="https://docs.microsoft.com/en-us/azure/templates/Microsoft.Compute/virtualMachines">this link</a>
        /// provides the schema for virtual machine properties.
        /// </summary>
        /// <param name="resource">The specific resource for which to retrieve the properties.</param>
        /// <param name="apiVersion">The ARM API version to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, and returning the resource properties.</returns>
        public async Task<ResourceProperties> GetResourcePropertiesAsync(ResourceIdentifier resource, string apiVersion, CancellationToken cancellationToken)
        {
            var client = this.GetResourceManagementClient(resource.SubscriptionId);

            // Get the resource type string
            if (!ResourceIdentifier.MapResourceTypeToString.TryGetValue(resource.ResourceType, out string resourceTypeString))
            {
                throw new ArgumentException($"Resource type {resource.ResourceType} is not supported for the GetResourcePropertiesAsync method", nameof(resource));
            }

            // Extract the provider and resource type
            ParseResourceTypeString(resourceTypeString, out string provider, out string type);

            // Get the default API version for this resource type or the latest API version if there is no default (or use a specific version if one was specified)
            if (string.IsNullOrEmpty(apiVersion))
            {
                if (ProviderToDefaultApiVersionMapping.ContainsKey(provider))
                {
                    apiVersion = ProviderToDefaultApiVersionMapping[provider];
                }
                else
                {
                    apiVersion = await this.GetLatestApiVersionAsync(client, provider, type, cancellationToken);
                }
            }

            // Get the resource
            GenericResourceInner result = await this.RunAndTrack(() => client.Resources.GetAsync(
                resource.ResourceGroupName,
                provider,
                string.Empty,
                type,
                resource.ResourceName,
                apiVersion,
                cancellationToken));

            // Get the resource properties as a JObject
            return new ResourceProperties(
                result.Sku == null ? null : new ResourceSku(result.Sku.Name, result.Sku.Tier, result.Sku.Size, result.Sku.Family, result.Sku.Model, result.Sku.Capacity),
                result.Properties as JObject,
                apiVersion);
        }

        /// <summary>
        /// Returns the Application Insights app ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The Application Insights resource identifier.
        /// The value of the <see cref="ResourceType"/> property must be equal to <see cref="ResourceType.ApplicationInsights"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the app ID.</returns>
        public async Task<string> GetApplicationInsightsAppIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken)
        {
            if (resourceIdentifier.ResourceType != ResourceType.ApplicationInsights)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceIdentifier), resourceIdentifier.ResourceType, $"The resource type must be {ResourceType.ApplicationInsights}");
            }

            // Extract the AppId from the resource properties
            ResourceProperties properties = await this.GetResourcePropertiesAsync(resourceIdentifier, cancellationToken);
            string applicationId = properties.Properties["AppId"].ToObject<string>();
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
                throw new ArgumentOutOfRangeException(nameof(resourceIdentifier), resourceIdentifier.ResourceType, $"The resource type must be {ResourceType.LogAnalytics}");
            }

            // Extract the workspace ID from the resource properties
            ResourceProperties properties = await this.GetResourcePropertiesAsync(resourceIdentifier, cancellationToken);
            string workspaceId = properties.Properties["customerId"].ToObject<string>();
            if (workspaceId == null)
            {
                throw new ArgumentException($"No workspace ID found for resource {resourceIdentifier.ResourceName}");
            }

            return workspaceId;
        }

        /// <summary>
        /// Executes an ARM GET request, using the specified resource, suffix, and query string.
        /// For example, the following call gets the list of databases for an SQL Server resource:
        /// <code>
        /// List&lt;JObject&gt; databases = await ExecuteArmQueryAsync(sqlResource, "/databases", "api-version=2017-10-01-preview", cancellationToken);
        /// </code>
        /// </summary>
        /// <param name="resource">The resource for the request.</param>
        /// <param name="suffix">The resource suffix.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>>A <see cref="Task{TResult}"/>, running the current operation, returning a list of all items returned.</returns>
        public async Task<List<JObject>> ExecuteArmQueryAsync(ResourceIdentifier resource, string suffix, string queryString, CancellationToken cancellationToken)
        {
            // Create relative path
            Uri relativePath = new Uri(resource.ToResourceId() + suffix + "?" + queryString, UriKind.Relative);
            return await this.ExecuteArmQueryAsync(relativePath, cancellationToken);
        }

        /// <summary>
        /// Executes an ARM GET request, using the relative path, and query string.
        /// For example, the following call gets the list of databases for an SQL Server resource:
        /// <code>
        /// List&lt;JObject&gt; databases = await ExecuteArmQueryAsync("subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01", cancellationToken);
        /// </code>
        /// </summary>
        /// <param name="relativePath">The reltive path of the query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>>A <see cref="Task{TResult}"/>, running the current operation, returning a list of all items returned.</returns>
        public async Task<List<JObject>> ExecuteArmQueryAsync(Uri relativePath, CancellationToken cancellationToken)
        {
            // Create the URI
            Uri nextLink = new Uri(this.baseUri, relativePath);
            List<JObject> allItems = new List<JObject>();

            while (nextLink != null)
            {
                this.tracer.TraceVerbose($"Sending a request to {nextLink}");
                HttpResponseMessage response = await this.httpRetryPolicy.RunAndTrackDependencyAsync(
                    this.tracer,
                    DependencyName,
                    "ExecuteArmQueryAsync",
                    async () =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, nextLink);

                        // Set the credentials
                        await this.credentials.ProcessHttpRequestAsync(request, cancellationToken);

                        // Send request and get the response as JObject
                        return await this.httpClientWrapper.SendAsync(request, TimeSpan.FromMinutes(HttpRequestTimeoutInMinutes), cancellationToken);
                    });

                if (!response.IsSuccessStatusCode)
                {
                    this.tracer.TraceError($"Query returned an error; Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Query returned an error code {response.StatusCode}");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                JObject responseObject = JObject.Parse(responseContent);

                const string valueKey = "value";
                IList<JObject> returnedObjects = null;
                if (responseObject.ContainsKey(valueKey))
                {
                    returnedObjects = responseObject[valueKey].ToObject<List<JObject>>();
                    allItems.AddRange(returnedObjects);
                }
                else
                {
                    allItems.Add(responseObject);
                }

                // Link to next page
                string nextLinkToken = responseObject.GetValue("nextLink", StringComparison.InvariantCulture)?.ToString();
                nextLink = (string.IsNullOrWhiteSpace(nextLinkToken) || !(returnedObjects?.Any() ?? false)) ? null : new Uri(nextLinkToken);
            }

            return allItems;
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
        /// Builds an OData query object, that filters the resources by the specified resource types.
        /// </summary>
        /// <param name="resourceTypes">The resource types to filter by.</param>
        /// <returns>The OData query object.</returns>
        private static ODataQuery<GenericResourceFilterInner> GetResourcesByTypeQuery(IEnumerable<ResourceType> resourceTypes)
        {
            // Convert the resource types to ARM strings
            List<string> resourceTypesStrings = new List<string>();
            foreach (ResourceType resourceType in resourceTypes)
            {
                if (!ResourceIdentifier.MapResourceTypeToString.TryGetValue(resourceType, out string resourceTypeString))
                {
                    throw new ArgumentException($"Resource type {resourceType} is not supported");
                }

                resourceTypesStrings.Add(resourceTypeString);
            }

            // Concatenate all the equality conditions with "or"
            string queryString = string.Join(" or ", resourceTypesStrings.Select(resourceType => "resourceType eq '" + resourceType.Replace("'", "''") + "'"));
            return new ODataQuery<GenericResourceFilterInner>(queryString);
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
        /// This method always returns the latest API version that is not a preview version.
        /// </summary>
        /// <param name="client">The ARM client.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="type">The resource type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the API version.</returns>
        private async Task<string> GetLatestApiVersionAsync(ResourceManagementClient client, string provider, string type, CancellationToken cancellationToken)
        {
            ProviderInner providerInformation = await this.GetProviderInformationAsync(client, provider, cancellationToken);
            ProviderResourceType providerResourceType = providerInformation.ResourceTypes.FirstOrDefault(resourceType => resourceType.ResourceType.Equals(type, StringComparison.CurrentCultureIgnoreCase));
            if (providerResourceType == null)
            {
                throw new ArgumentException($"Provider {provider} does not support type {type}");
            }

            return providerResourceType.ApiVersions.Where(version => version.Contains("preview") == false).Max();
        }

        /// <summary>
        /// Creates a new subscription client
        /// </summary>
        /// <returns>The subscription client</returns>
        private SubscriptionClient GetSubscriptionClient()
        {
            var restClient = RestClient.Configure().WithBaseUri(this.baseUri.ToString()).WithCredentials(this.credentials).Build();

            return new SubscriptionClient(restClient);
        }

        /// <summary>
        /// Creates a new resource manager client and initializes it with the specified subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <returns>The resource manager client</returns>
        private ResourceManagementClient GetResourceManagementClient(string subscriptionId)
        {
            var restClient = RestClient.Configure().WithBaseUri(this.baseUri.ToString()).WithCredentials(this.credentials).Build();

            return new ResourceManagementClient(restClient) { SubscriptionId = subscriptionId };
        }

        /// <summary>
        /// Enumerate all results, using the specified paging functions.
        /// We are not passing to this method the cancellation token as the <paramref name="firstPage"/> function responsible for it.
        /// </summary>
        /// <typeparam name="T">The type of item returned in the paged results</typeparam>
        /// <param name="firstPage">A function that returns the first results page</param>
        /// <param name="nextPage">A function that returns the next results page, given the next page link</param>
        /// <returns>The list of items, read from the paged results</returns>
        private async Task<List<T>> ReadAllPages<T>(Func<Task<IPage<T>>> firstPage, Func<string, Task<IPage<T>>> nextPage)
        {
            List<T> items = new List<T>();
            IPage<T> currentPage = await firstPage();
            while (currentPage != null)
            {
                // Read all items from the current page
                int prevCount = items.Count;
                items.AddRange(currentPage);
                int currentPageCount = items.Count - prevCount;

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
            return this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                commandName,
                async () =>
                {
                    try
                    {
                        return await dependencyCall();
                    }
                    catch (CloudException e)
                    {
                        if (e.Response != null)
                        {
                            throw new AzureResourceManagerClientException(e.Response.StatusCode, e.Response.ReasonPhrase, e);
                        }
                        else
                        {
                            throw new AzureResourceManagerClientException(e.Message, e);
                        }
                    }
                });
        }
    }
}