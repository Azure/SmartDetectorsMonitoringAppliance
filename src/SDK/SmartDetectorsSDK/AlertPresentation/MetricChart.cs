//-----------------------------------------------------------------------
// <copyright file="MetricChart.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a chart displaying metric data.
    /// </summary>
    public class MetricChart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChart"/> class.
        /// </summary>
        /// <param name="metricName">The name of the metric to display in the chart.</param>
        /// <param name="timeGrain">The time grain (resolution) of the metric to display.</param>
        /// <param name="aggregationType">The aggregation type to use when displaying the metric.</param>
        public MetricChart(
            string metricName,
            TimeSpan timeGrain,
            AggregationType aggregationType)
        {
            this.MetricName = metricName;
            this.TimeGrain = timeGrain;
            this.AggregationType = aggregationType;
            this.MetricDimensions = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets an optional resource ID to query for the metric. If this is <c>null</c> or empty,
        /// then the chart will display the metric from the alert's resource.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets the name of the metric to display in the chart.
        /// </summary>
        public string MetricName { get; }

        /// <summary>
        /// Gets or sets an optional namespace for the metric to display. If this is <c>null</c> or empty,
        /// then the metric's namespace will be retrieved from the <see cref="ResourceId"/>'s provider namespace.
        /// For all Azure platform metrics this should remain empty.
        /// </summary>
        public string MetricNamespace { get; set; }

        /// <summary>
        /// Gets optional dimensions to use when querying the metric data.
        /// </summary>
        public Dictionary<string, string> MetricDimensions { get; }

        /// <summary>
        /// Gets or sets an optional start time in UTC to display in the chart. If not specified, the
        /// chart's start time will be based on the alert's time.
        /// </summary>
        public DateTime? StartTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets an optional end time in UTC to display in the chart. If not specified, the
        /// chart's end time will be based on the alert's time.
        /// </summary>
        public DateTime? EndTimeUtc { get; set; }

        /// <summary>
        /// Gets the time grain (resolution) of the metric to display.
        /// </summary>
        public TimeSpan TimeGrain { get; }

        /// <summary>
        /// Gets the aggregation type to use when displaying the metric.
        /// </summary>
        public AggregationType AggregationType { get; }

        /// <summary>
        /// Gets or sets the type of the threshold to show, this applies both to
        /// <see cref="StaticThreshold"/> and <see cref="DynamicThreshold"/> (if
        /// specified).
        /// </summary>
        public ThresholdType ThresholdType { get; set; }

        /// <summary>
        /// Gets or sets an optional static threshold to show in the chart.
        /// </summary>
        public StaticThreshold StaticThreshold { get; set; }

        /// <summary>
        /// Gets or sets an optional dynamic threshold to show in the chart.
        /// </summary>
        public DynamicThreshold DynamicThreshold { get; set; }
    }
}
