//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;

    /// <summary>
    /// An implementation of the <see cref="IAnalysisServicesFactory"/> interface.
    /// </summary>
    public class AnalysisServicesFactory : IInternalAnalysisServicesFactory
    {
        private readonly IExtendedTracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ICredentialsFactory credentialsFactory;
        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;
        private readonly IQueryRunInfoProvider queryRunInfoProvider;
        private readonly TimeSpan queryTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisServicesFactory"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper.</param>
        /// <param name="credentialsFactory">The credentials factory.</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client.</param>
        /// <param name="queryRunInfoProvider">The query run information provider.</param>
        public AnalysisServicesFactory(IExtendedTracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, IExtendedAzureResourceManagerClient azureResourceManagerClient, IQueryRunInfoProvider queryRunInfoProvider)
        {
            this.tracer = tracer;
            this.httpClientWrapper = httpClientWrapper;
            this.credentialsFactory = credentialsFactory;
            this.azureResourceManagerClient = azureResourceManagerClient;
            this.queryRunInfoProvider = queryRunInfoProvider;

            // string timeoutString = ConfigurationReader.ReadConfig("AnalyticsQueryTimeoutInMinutes", required: true);
            string timeoutString = "15";
            this.queryTimeout = TimeSpan.FromMinutes(int.Parse(timeoutString, CultureInfo.InvariantCulture));

            this.UsedLogAnalysisClient = false;
            this.UsedMetricClient = false;
        }

        /// <summary>
        /// Gets a value indicating whether a log analysis client was used
        /// </summary>
        public bool UsedLogAnalysisClient { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a log metric client was used
        /// </summary>
        public bool UsedMetricClient { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in log analytics workspaces.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A log analytics telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on log analytics workspaces.</returns>
        public async Task<ITelemetryDataClient> CreateLogAnalyticsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Mark that a log signal was used to create the alert
            this.UsedLogAnalysisClient = true;

            // Get the query run info, and verify it
            QueryRunInfo runInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(resources, cancellationToken);
            VerifyRunInfo(runInfo, TelemetryDbType.LogAnalytics);

            // Create the client
            return new LogAnalyticsTelemetryDataClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, this.azureResourceManagerClient, runInfo.ResourceIds, this.queryTimeout);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in Application Insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">An Application Insights telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on Application Insights.</returns>
        public async Task<ITelemetryDataClient> CreateApplicationInsightsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Mark that a log signal was used to create the alert
            this.UsedLogAnalysisClient = true;

            // Get the query run info, and verify it
            QueryRunInfo runInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(resources, cancellationToken);
            VerifyRunInfo(runInfo, TelemetryDbType.ApplicationInsights);

            // Create the client
            return new ApplicationInsightsTelemetryDataClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, this.azureResourceManagerClient, runInfo.ResourceIds, this.queryTimeout);
        }

        /// <summary>
        /// Creates an instance of <see cref="IMetricClient"/>, used to fetch the resource metrics.
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The metric client, that can be used to fetch the resource metrics.</returns>
        public Task<IMetricClient> CreateMetricClientAsync(string subscriptionId, CancellationToken cancellationToken)
        {
            // Mark that a metric signal was used to create the alert
            this.UsedMetricClient = true;

            // Create the client
            return Task.FromResult<IMetricClient>(new MetricClient(this.tracer, this.credentialsFactory, subscriptionId));
        }

        /// <summary>
        /// Creates an instance of <see cref="IAzureResourceManagerClient"/>, used to fetch resource details from ARM.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Azure Resource Manager client, that can be used to fetch resource details from ARM.</returns>
        public Task<IAzureResourceManagerClient> CreateArmClientAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IAzureResourceManagerClient>(this.azureResourceManagerClient);
        }

        /// <summary>
        /// Creates an instance of <see cref="IActivityLogClient"/>, used to fetch resource activity from Activity Log.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Activity Log client, that can be used to fetch the resource activity from Activity Log.</returns>
        public Task<IActivityLogClient> CreateActivityLogClientAsync(CancellationToken cancellationToken)
        {
            // Create the client
            return Task.FromResult<IActivityLogClient>(new ActivityLogClient(this.credentialsFactory, this.httpClientWrapper, this.tracer));
        }

        /// <summary>
        /// Perform basic validations on the specified query run information.
        /// </summary>
        /// <param name="runInfo">The query run information</param>
        /// <param name="expectedType">The expected telemetry DB type</param>
        private static void VerifyRunInfo(QueryRunInfo runInfo, TelemetryDbType expectedType)
        {
            // Verify the telemetry DB type
            if (runInfo.Type != expectedType)
            {
                throw new TelemetryDataClientCreationException($"Telemetry client creation failed - telemetry resource type is {runInfo.Type}");
            }

            // Verify that the resource IDs are not empty
            if (!runInfo.ResourceIds.Any())
            {
                throw new TelemetryDataClientCreationException("Telemetry client creation failed - no resources found");
            }
        }
    }
}