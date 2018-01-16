//-----------------------------------------------------------------------
// <copyright file="IAzureResourceManagerClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Common
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An interface for azure resource manager client
    /// </summary>
    public interface IAzureResourceManagerClient
    {
        /// <summary>
        /// Gets the resource ID that represents the resource identified by the specified <see cref="ResourceIdentifier"/> structure.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceIdentifier">The <see cref="ResourceIdentifier"/> structure.</param>
        /// <returns>The resource ID.</returns>
        string GetResourceId(ResourceIdentifier resourceIdentifier);

        /// <summary>
        /// Gets the <see cref="ResourceIdentifier"/> structure that represents the resource identified by the specified resource ID.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
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
        Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources of the specified types in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInSubscriptionAsync(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources in the specified resource group.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceGroupName">The resource group name.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroupAsync(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<string>> GetAllSubscriptionIdsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the resource properties, as a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource properties.</returns>
        Task<JObject> GetResourcePropertiesAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the application insights app ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The application insights resource identifier.
        /// The value of the <see cref="ResourceIdentifier.ResourceType"/> property must be equal to <see cref="ResourceType.ApplicationInsights"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the app ID.</returns>
        Task<string> GetApplicationInsightsAppIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<SubscriptionInner>> GetAllSubscriptionsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the log analytics workspace ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The log analytics resource identifier.
        /// The value of the <see cref="ResourceIdentifier.ResourceType"/> property must be equal to <see cref="ResourceType.LogAnalytics"/>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the workspace ID.</returns>
        Task<string> GetLogAnalyticsWorkspaceIdAsync(ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken);
    }
}