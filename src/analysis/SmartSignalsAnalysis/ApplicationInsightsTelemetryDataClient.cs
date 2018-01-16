//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsTelemetryDataClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartSignals.Common;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An implementation of <see cref="ITelemetryDataClient"/> to access application insights resources.
    /// </summary>
    public class ApplicationInsightsTelemetryDataClient : TelemetryDataClientBase
    {
        private const string UriFormat = "https://api.applicationinsights.io/v1/apps/{0}/query";

        private readonly string applicationId;
        private readonly IReadOnlyList<string> applicationsResourceIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsTelemetryDataClient"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="applicationId">The application Id on which the queries will run. If there are multiple applications, this should be the ID of one of them.</param>
        /// <param name="applicationsResourceIds">
        ///     The resource IDs of the applications on which the queries will run.
        ///     Can be null or empty if there is only one application to analyze. If there are multiple applications,
        ///     one of these IDs need to match the application identified by the specified <paramref name="applicationId"/>. 
        /// </param>
        /// <param name="queryTimeout">The query timeout.</param>
        public ApplicationInsightsTelemetryDataClient(ITracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, string applicationId, IEnumerable<string> applicationsResourceIds, TimeSpan queryTimeout)
            : base(tracer, httpClientWrapper, credentialsFactory, new Uri(string.Format(UriFormat, applicationId)), queryTimeout, "ApplicationInsights")
        {
            this.applicationId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => applicationId);
            this.applicationsResourceIds = applicationsResourceIds?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Update the HTTP request content with required values.
        /// </summary>
        /// <param name="requestContent">The request content.</param>
        protected override void UpdateRequestContent(JObject requestContent)
        {
            if (this.applicationsResourceIds.Count > 1)
            {
                requestContent["applications"] = new JArray(this.applicationsResourceIds);
            }
        }
    }
}