namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Rest.Azure;
    using Microsoft.Rest.Azure.OData;

    /// <summary>
    /// Implementation of the <see cref="IAzureResourceManagerClient"/> interface
    /// </summary>
    public class AzureResourceManagerClient : IAzureResourceManagerClient
    {
        /// <summary>
        /// The maximal number of allowed resources to enumerate
        /// </summary>
        private const int MaxResourcesToEnumerate = 100;

        private const string SubscriptionRegexPattern = "/subscriptions/(?<subscriptionId>.*)";
        private const string ResourceGroupRegexPattern = SubscriptionRegexPattern + "/resourceGroups/(?<resourceGroupName>.*)";
        private const string ResourceRegexPattern = ResourceGroupRegexPattern + "/providers/(?<resourceProviderAndType>.*)/(?<resourceName>.*)";

        /// <summary>
        /// A dictionary, mapping <see cref="ResourceType"/> enumeration values to matching ARM string
        /// </summary>
        private static readonly Dictionary<ResourceType, string> MapResourceTypeToString = new Dictionary<ResourceType, string>()
        {
            [ResourceType.VirtualMachine] = "Microsoft.Compute/virtualMachines"
        };

        /// <summary>
        /// A dictionary, mapping ARM strings to their matching <see cref="ResourceType"/> enumeration values
        /// </summary>
        private static readonly Dictionary<string, ResourceType> MapStringToResourceType = MapResourceTypeToString.ToDictionary(x => x.Value, x => x.Key, StringComparer.InvariantCultureIgnoreCase);

        private readonly AzureCredentials credentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClient"/> class
        /// </summary>
        public AzureResourceManagerClient()
        {
            this.credentials = new AzureCredentialsFactory().FromMSI(AzureEnvironment.AzureGlobalCloud);
        }

        /// <summary>
        /// Gets the resource ID that represents the resource identified by the specified <see cref="ResourceIdentifier"/> structure.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
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
            pattern = pattern.Replace("(?<subscriptionId>.*)", resourceIdentifier.SubscriptionId);
            if (resourceIdentifier.ResourceType != ResourceType.Subscription)
            {
                pattern = pattern.Replace("(?<resourceGroupName>.*)", resourceIdentifier.ResourceGroupName);
                if (resourceIdentifier.ResourceType != ResourceType.ResourceGroup)
                {
                    pattern = pattern.Replace("(?<resourceProviderAndType>.*)", resourceProviderAndType);
                    pattern = pattern.Replace("(?<resourceName>.*)", resourceIdentifier.ResourceName);
                }
            }

            return pattern;
        }

        /// <summary>
        /// Gets the <see cref="ResourceIdentifier"/> structure that represents the resource identified by the specified resource ID.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
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
        public async Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscription(string subscriptionId, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            Task<IPage<ResourceGroupInner>> FirstPage() => resourceManagementClient.ResourceGroups.ListAsync(cancellationToken: cancellationToken);
            Task<IPage<ResourceGroupInner>> NextPage(string nextPageLink) => resourceManagementClient.ResourceGroups.ListNextAsync(nextPageLink, cancellationToken);
            return (await this.ReadAllPages(FirstPage, NextPage, "resource groups in subscription"))
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
        public async Task<IList<ResourceIdentifier>> GetAllResourcesInSubscription(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            List<string> resourceTypesStrings = this.ConvertResourceTypes(resourceTypes);
            ODataQuery<GenericResourceFilterInner> query = new ODataQuery<GenericResourceFilterInner>(resource => resourceTypesStrings.Contains(resource.ResourceType));
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.Resources.ListAsync(query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.Resources.ListNextAsync(nextPageLink, cancellationToken);
            return (await this.ReadAllPages(FirstPage, NextPage, "virtual machines in subscription"))
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
        public async Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroup(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken)
        {
            ResourceManagementClient resourceManagementClient = this.GetResourceManagementClient(subscriptionId);
            List<string> resourceTypesStrings = this.ConvertResourceTypes(resourceTypes);
            ODataQuery<GenericResourceFilterInner> query = new ODataQuery<GenericResourceFilterInner>(resource => resourceTypesStrings.Contains(resource.ResourceType));
            Task<IPage<GenericResourceInner>> FirstPage() => resourceManagementClient.ResourceGroups.ListResourcesAsync(resourceGroupName, query, cancellationToken);
            Task<IPage<GenericResourceInner>> NextPage(string nextPageLink) => resourceManagementClient.ResourceGroups.ListResourcesNextAsync(nextPageLink, cancellationToken);
            return (await this.ReadAllPages(FirstPage, NextPage, "virtual machines in resource group"))
                .Select(resource => this.GetResourceIdentifier(resource.Id))
                .ToList();
        }

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        public async Task<IList<string>> GetAllSubscriptionIds(CancellationToken cancellationToken = default(CancellationToken))
        {
            var subscriptions = await this.GetSubscriptionClient().Subscriptions.ListAsync(cancellationToken);
            return subscriptions.Select(subscription => subscription.SubscriptionId).ToList();
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
        /// Converts the collection of <see cref="ResourceType"/> enumeration values
        /// to a collection of their matching ARM strings.
        /// </summary>
        /// <param name="resourceTypes">The resource types.</param>
        /// <returns>The ARM strings.</returns>
        private List<string> ConvertResourceTypes(IEnumerable<ResourceType> resourceTypes)
        {
            List<string> resourceTypesStrings = new List<string>();
            foreach (ResourceType resourceType in resourceTypes)
            {
                if (!MapResourceTypeToString.TryGetValue(resourceType, out string resourceTypeString))
                {
                    throw new ArgumentException($"Resource type {resourceType} is not supported");
                }

                resourceTypesStrings.Add(resourceTypeString);
            }

            return resourceTypesStrings;
        }
    }
}