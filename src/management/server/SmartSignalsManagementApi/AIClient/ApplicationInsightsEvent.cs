//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsEvent.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the Application Insights event data model.
    /// As we don't need all the fields, we only put the required ones.
    /// </summary>
    public class ApplicationInsightsEvent
    {
        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the event custom dimensions.
        /// </summary>
        [JsonProperty("customDimensions")]
        public Dictionary<string, string> CustomDimensions { get; set; }

        /// <summary>
        /// Gets or sets the event custom measurements.
        /// </summary>
        [JsonProperty("customMeasurements")]
        public Dictionary<string, double> CustomMeasurements { get; set; }
    }
}