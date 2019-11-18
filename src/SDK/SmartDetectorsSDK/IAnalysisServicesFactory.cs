//-----------------------------------------------------------------------
// <copyright file="IAnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
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
        /// Creates an instance of <see cref="ILogAnalyticsClient"/>, used for running Log Analytics queries on the specified resource. If the
        /// resource type of <paramref name="resource"/> is <see cref="ResourceType.ApplicationInsights"/> then the created client will query
        /// Application Insights telemetry.
        /// </summary>
        /// <param name="resource">The resource to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A Log Analytics client could not be created for the specified resource.</exception>
        /// <returns>The Log Analytics client, that can be used to run queries on <paramref name="resource"/>.</returns>
        Task<ILogAnalyticsClient> CreateLogAnalyticsClientAsync(ResourceIdentifier resource, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an instance of <see cref="ILogAnalyticsClient"/>, used for running queries on the specified Application Insights telemetry.
        /// </summary>
        /// <param name="applicationId">The application Id of the Application Insights resource to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A Log Analytics client could not be created for the specified resource.</exception>
        /// <returns>The Log Analytics client, that can be used to run queries on the Application Insights telemetry.</returns>
        Task<ILogAnalyticsClient> CreateLogAnalyticsClientAsync(string applicationId, CancellationToken cancellationToken);

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