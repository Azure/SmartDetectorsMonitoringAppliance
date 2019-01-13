//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// An implementation of the <see cref="IAnalysisServicesFactory"/> interface.
    /// </summary>
    public class AnalysisServicesFactory : IInternalAnalysisServicesFactory
    {
        private const int MaxNumberOfResourcesInQuery = 100;
        private readonly ConcurrentDictionary<string, IList<ResourceIdentifier>> subscriptionIdToWorkspaces = new ConcurrentDictionary<string, IList<ResourceIdentifier>>(StringComparer.CurrentCultureIgnoreCase);
        private readonly IExtendedTracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ICredentialsFactory credentialsFactory;
        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;
        private readonly TimeSpan queryTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisServicesFactory"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper.</param>
        /// <param name="credentialsFactory">The credentials factory.</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client.</param>
        public AnalysisServicesFactory(IExtendedTracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, IExtendedAzureResourceManagerClient azureResourceManagerClient)
        {
            this.tracer = tracer;
            this.httpClientWrapper = httpClientWrapper;
            this.credentialsFactory = credentialsFactory;
            this.azureResourceManagerClient = azureResourceManagerClient;

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
            // Verify that there are no Application Insights resources
            if (resources.Any(resource => resource.ResourceType == ResourceType.ApplicationInsights))
            {
                throw new TelemetryDataClientCreationException($"Telemetry client creation failed - resources shouldn't contain resources of the type {ResourceType.ApplicationInsights}");
            }

            // Mark that a log signal was used to create the alert
            this.UsedLogAnalysisClient = true;

            // Get the resource IDs
            IReadOnlyList<string> resourceIds = await this.GetResourceIdsForLogAnalyticsAsync(resources, cancellationToken);

            // Create the client
            return new LogAnalyticsTelemetryDataClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, this.azureResourceManagerClient, resourceIds, this.queryTimeout);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITelemetryDataClient"/>, used for running queries against data in Application Insights.
        /// </summary>
        /// <param name="resources">The list of resources to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">An Application Insights telemetry data client could not be created for the specified resources.</exception>
        /// <returns>The telemetry data client, that can be used to run queries on Application Insights.</returns>
        public Task<ITelemetryDataClient> CreateApplicationInsightsTelemetryDataClientAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Verify that there are resources
            if (!resources.Any())
            {
                throw new TelemetryDataClientCreationException("No resources provided");
            }

            // Verify that all resources are of type ApplicationInsights
            if (resources.Any(resource => resource.ResourceType != ResourceType.ApplicationInsights))
            {
                throw new TelemetryDataClientCreationException($"Telemetry client creation failed - all resources must be of type {ResourceType.ApplicationInsights}");
            }

            // Verify there are not too many resources
            if (resources.Count > MaxNumberOfResourcesInQuery)
            {
                throw new TelemetryDataClientCreationException($"Cannot run analysis on more than {MaxNumberOfResourcesInQuery} applications");
            }

            List<string> resourceIds = resources.Select(application => application.ToResourceId()).ToList();

            // Mark that a log signal was used to create the alert
            this.UsedLogAnalysisClient = true;

            // Create the client
            return Task.FromResult<ITelemetryDataClient>(new ApplicationInsightsTelemetryDataClient(
                this.tracer, this.httpClientWrapper, this.credentialsFactory, this.azureResourceManagerClient, resourceIds, this.queryTimeout));
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
        /// Gets IDs of Log Analytics resources.
        /// </summary>
        /// <param name="resources">The resources</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource IDs</returns>
        private async Task<IReadOnlyList<string>> GetResourceIdsForLogAnalyticsAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Verify that there are resources
            if (!resources.Any())
            {
                throw new TelemetryDataClientCreationException("No resources provided");
            }

            IReadOnlyList<ResourceIdentifier> workspaces;
            if (resources.All(resource => resource.ResourceType == ResourceType.LogAnalytics))
            {
                // All resources are of type LogAnalytics. Create a client that queries all these workspaces.
                workspaces = resources;
            }
            else
            {
                // Since we do not know where the telemetry of each resource is, create a client that queries all workspaces in the subscription.
                var workspacesList = new List<ResourceIdentifier>();
                foreach (string subscriptionId in resources.Select(resource => resource.SubscriptionId).Distinct(StringComparer.CurrentCultureIgnoreCase))
                {
                    // Try to get the workspaces list from the cache, and if it isn't there, use the Azure Resource Manager client
                    IList<ResourceIdentifier> subscriptionWorkspaces;
                    if (!this.subscriptionIdToWorkspaces.TryGetValue(subscriptionId, out subscriptionWorkspaces))
                    {
                        subscriptionWorkspaces = await this.azureResourceManagerClient.GetAllResourcesInSubscriptionAsync(subscriptionId, new[] { ResourceType.LogAnalytics }, cancellationToken);
                        this.subscriptionIdToWorkspaces[subscriptionId] = subscriptionWorkspaces;
                    }

                    workspacesList.AddRange(subscriptionWorkspaces);
                }

                workspaces = workspacesList;
                if (workspaces.Count == 0)
                {
                    throw new TelemetryDataClientCreationException("No log analytics workspaces were found");
                }
            }

            // Verify there aren't too many resources
            if (workspaces.Count > MaxNumberOfResourcesInQuery)
            {
                throw new TelemetryDataClientCreationException($"Cannot run analysis on more than {MaxNumberOfResourcesInQuery} workspaces");
            }

            List<string> workspacesResourceIds = workspaces.Select(workspace => workspace.ToResourceId()).ToList();
            return workspacesResourceIds;
        }
    }
}