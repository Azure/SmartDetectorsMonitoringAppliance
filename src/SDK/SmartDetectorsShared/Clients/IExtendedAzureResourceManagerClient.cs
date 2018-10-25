//-----------------------------------------------------------------------
// <copyright file="IExtendedAzureResourceManagerClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;

    /// <summary>
    /// An interface for Azure Resource Manager client
    /// </summary>
    public interface IExtendedAzureResourceManagerClient : IAzureResourceManagerClient
    {
        /// <summary>
        /// Enumerates all the accessible subscriptions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the subscription IDs</returns>
        Task<IList<string>> GetAllSubscriptionIdsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the Application Insights app ID.
        /// </summary>
        /// <param name="resourceIdentifier">
        /// The Application Insights resource identifier.
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
        Task<IList<AzureSubscription>> GetAllSubscriptionsAsync(CancellationToken cancellationToken = default(CancellationToken));

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