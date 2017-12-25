//-----------------------------------------------------------------------
// <copyright file="IAnalysisServicesFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for exposing a factory that creates analysis services used for querying
    /// telemetry data by the signal.
    /// </summary>
    public interface IAnalysisServicesFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IQueryClient"/>, used for running queries against data in log analytics workspaces.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="QueryClientCreationException">A log analytics query client could not be created for the specified resources.</exception>
        /// <returns>The query client, that can be used to run queries on log analytics workspaces.</returns>
        Task<IQueryClient> CreateLogAnalyticsQueryClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="IQueryClient"/>, used for running queries against data in application insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="QueryClientCreationException">An application insights query client could not be created for the specified resources.</exception>
        /// <returns>The query client, that can be used to run queries on application insights.</returns>
        Task<IQueryClient> CreateApplicationInsightsQueryClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);
    }
}