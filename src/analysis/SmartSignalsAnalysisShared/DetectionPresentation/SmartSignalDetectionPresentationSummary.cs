//-----------------------------------------------------------------------
// <copyright file="SmartSignalDetectionPresentationSummary.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis.DetectionPresentation
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class holds presentation information of the detection summary (card)
    /// </summary>
    public class SmartSignalDetectionPresentationSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalDetectionPresentationSummary"/> class
        /// </summary>
        /// <param name="value">The summary value</param>
        /// <param name="details">The summary details</param>
        /// <param name="chart">The summary chart</param>
        public SmartSignalDetectionPresentationSummary(string value, string details, SmartSignalDetectionPresentationProperty chart)
        {
            this.Value = value;
            this.Details = details;
            this.Chart = chart;
        }

        /// <summary>
        /// Gets the summary value
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; }

        /// <summary>
        /// Gets the summary details
        /// </summary>
        [JsonProperty("details")]
        public string Details { get; }

        /// <summary>
        /// Gets the summary chart
        /// </summary>
        [JsonProperty("chart")]
        public SmartSignalDetectionPresentationProperty Chart { get; }
    }
}