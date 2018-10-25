//-----------------------------------------------------------------------
// <copyright file="IAnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;

    /// <summary>
    /// An interface for exposing a factory that creates analysis services used for querying
    /// telemetry data by the Smart Detector.
    /// </summary>
    public interface IAnalysisServicesFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in log analytics workspaces.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A log analytics telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on log analytics workspaces.</returns>
        Task<ITelemetryDataClient> CreateLogAnalyticsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in Application Insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">An Application Insights telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on Application Insights.</returns>
        Task<ITelemetryDataClient> CreateApplicationInsightsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="IMetricClient"/>, used to fetch resource metrics.
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, returning the metric client.</returns>
        Task<IMetricClient> CreateMetricClientAsync(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="IAzureResourceManagerClient"/>, used to fetch resource details from ARM.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, returning the Azure Resource Manager client.</returns>
        Task<IAzureResourceManagerClient> CreateArmClientAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="IActivityLogClient"/>, used to fetch resource activity from Activity Log.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Activity Log client, that can be used to fetch the resource activity from Activity Log</returns>
        Task<IActivityLogClient> CreateActivityLogClientAsync(CancellationToken cancellationToken);
    }
}