//-----------------------------------------------------------------------
// <copyright file="TelemetryDbType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum for the type of DB on which to run queries that are part of alerts presentation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryDbType
    {
        /// <summary>
        /// The query should run on a log analytics workspace
        /// </summary>
        LogAnalytics,

        /// <summary>
        /// The query should run on am Application Insights app
        /// </summary>
        ApplicationInsights
    }
}
