//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// An implementation of the <see cref="IAnalysisServicesFactory"/> interface.
    /// </summary>
    public class AnalysisServicesFactory : IInternalAnalysisServicesFactory
    {
        private const string ApplicationInsightsQueryUriFormat = "https://api.applicationinsights.io/v1/apps/{0}/query";
        private const string LogAnalyticsQueryUriFormat = "https://api.loganalytics.io/v1/workspaces/{0}/query";
        private const string ResourceCentricQueryUriFormat = "https://api.loganalytics.io/v1{0}/query";

        private readonly ITracer tracer;
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
        public AnalysisServicesFactory(ITracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, IExtendedAzureResourceManagerClient azureResourceManagerClient)
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
        /// Creates an instance of <see cref="ILogAnalyticsClient"/>, used for running Log Analytics queries on the specified resource. If the
        /// resource type of <paramref name="resource"/> is <see cref="ResourceType.ApplicationInsights"/> then the created client will query
        /// Application Insights telemetry.
        /// </summary>
        /// <param name="resource">The resource to analyze.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="TelemetryDataClientCreationException">A Log Analytics client could not be created for the specified resource.</exception>
        /// <returns>The Log Analytics client, that can be used to run queries on <paramref name="resource"/>.</returns>
        public async Task<ILogAnalyticsClient> CreateLogAnalyticsClientAsync(ResourceIdentifier resource, CancellationToken cancellationToken)
        {
            // Mark that a log signal was used to create the alert
            this.UsedLogAnalysisClient = true;

            // Split the flow according the resource type - Application Insights and Log Analytics workspace resources have their own query endpoints
            string uriFormat;
            string resourceId;
            switch (resource.ResourceType)
            {
                case ResourceType.ApplicationInsights:
                    uriFormat = ConfigurationManager.AppSettings["ApplicationInsightsQueryUriFormat"] ?? ApplicationInsightsQueryUriFormat;
                    resourceId = await this.azureResourceManagerClient.GetApplicationInsightsAppIdAsync(resource, cancellationToken);
                    break;

                case ResourceType.LogAnalytics:
                    uriFormat = ConfigurationManager.AppSettings["LogAnalyticsQueryUriFormat"] ?? LogAnalyticsQueryUriFormat;
                    resourceId = await this.azureResourceManagerClient.GetLogAnalyticsWorkspaceIdAsync(resource, cancellationToken);
                    break;

                default:
                    uriFormat = ConfigurationManager.AppSettings["ResourceCentricQueryUriFormat"] ?? ResourceCentricQueryUriFormat;
                    resourceId = resource.ToResourceId();
                    break;
            }

            // And create the client
            var queryUri = new Uri(string.Format(CultureInfo.InvariantCulture, uriFormat, resourceId));
            return new LogAnalyticsClient(this.tracer, this.httpClientWrapper, this.credentialsFactory, queryUri, this.queryTimeout);
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
            return Task.FromResult<IMetricClient>(new MetricClient(this.tracer, this.credentialsFactory));
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
    }
}