//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItemPresentationSummary.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.SignalResultPresentation
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class holds presentation information of the Smart Signal result item summary (card)
    /// </summary>
    public class SmartSignalResultItemPresentationSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultItemPresentationSummary"/> class
        /// </summary>
        /// <param name="value">The summary value</param>
        /// <param name="details">The summary details</param>
        /// <param name="chart">The summary chart</param>
        public SmartSignalResultItemPresentationSummary(string value, string details, SmartSignalResultItemPresentationProperty chart)
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
        public SmartSignalResultItemPresentationProperty Chart { get; }
    }
}