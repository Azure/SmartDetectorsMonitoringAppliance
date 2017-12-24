namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for azure resource manager client
    /// </summary>
    public interface IAzureResourceManagerClient
    {
        /// <summary>
        /// Gets the resource ID that represents the resource identified by the specified <see cref="ResourceIdentifier"/> structure.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceIdentifier">The <see cref="ResourceIdentifier"/> structure.</param>
        /// <returns>The resource ID.</returns>
        string GetResourceId(ResourceIdentifier resourceIdentifier);

        /// <summary>
        /// Gets the <see cref="ResourceIdentifier"/> structure that represents the resource identified by the specified resource ID.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceId">The resource ID</param>
        /// <returns>The <see cref="ResourceIdentifier"/> structure.</returns>
        ResourceIdentifier GetResourceIdentifier(string resourceId);

        /// <summary>
        /// Enumerates all the resource groups in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource groups.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscription(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources of the specified types in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInSubscription(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources in the specified resource group.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceGroupName">The resource group name.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroup(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<string>> GetAllSubscriptionIds(CancellationToken cancellationToken = default(CancellationToken));
    }
}