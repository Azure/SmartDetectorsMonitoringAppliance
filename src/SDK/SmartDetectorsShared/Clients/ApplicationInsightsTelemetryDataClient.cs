//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsTelemetryDataClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An implementation of <see cref="ITelemetryDataClient"/> to access Application Insights resources.
    /// </summary>
    public class ApplicationInsightsTelemetryDataClient : TelemetryDataClientBase
    {
        private const string UriFormat = "https://api.applicationinsights.io/v1/apps/{0}/query";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsTelemetryDataClient"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="applicationsResourceIds">The resource IDs of the applications on which the queries will run.</param>
        /// <param name="queryTimeout">The query timeout.</param>
        public ApplicationInsightsTelemetryDataClient(
            IExtendedTracer tracer,
            IHttpClientWrapper httpClientWrapper,
            ICredentialsFactory credentialsFactory,
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            IEnumerable<string> applicationsResourceIds,
            TimeSpan queryTimeout)
            : base(
                tracer,
                httpClientWrapper,
                credentialsFactory,
                azureResourceManagerClient,
                ConfigurationManager.AppSettings["ApplicationInsightsUriFormat"] ?? UriFormat,
                queryTimeout,
                TelemetryDbType.ApplicationInsights,
                applicationsResourceIds)
        {
        }

        /// <summary>
        /// Gets the key in the Draft request for additional applications
        /// </summary>
        protected override string AdditionalTelemetryResourceIdsRequestKey => "applications";

        /// <summary>
        /// Get the id of the telemetry resource
        /// </summary>
        /// <param name="telemetryResource">The telemetry resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the telemetry resource id.</returns>
        protected override Task<string> GetTelemetryResourceIdAsync(ResourceIdentifier telemetryResource, CancellationToken cancellationToken)
        {
            return this.AzureResourceManagerClient.GetApplicationInsightsAppIdAsync(telemetryResource, cancellationToken);
        }
    }
}