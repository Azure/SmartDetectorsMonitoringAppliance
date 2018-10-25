//-----------------------------------------------------------------------
// <copyright file="MetricQueryResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using System.Collections.Generic;

    /// <summary>
    /// An object which represents an metric query result for a single metric.
    /// </summary>
    public class MetricQueryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricQueryResult"/> class
        /// </summary>
        /// <param name="name">The metric's name</param>
        /// <param name="unit">The metric's value unit (Seconds, bytes, etc.)</param>
        /// <param name="timeseries">The metric's time series</param>
        public MetricQueryResult(string name, string unit, IReadOnlyList<MetricTimeSeries> timeseries)
        {
            this.Name = name;
            this.Unit = unit;
            this.Timeseries = timeseries;
        }

        /// <summary>
        /// Gets the metric's name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the metric's value unit (Seconds, bytes, etc.)
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// Gets the metric's time series.
        /// A timeseries is created per dimension value, if a filter by dimension's value is set
        /// E.g., if we set <c>filter = "ApiName eq 'PutMessage' or ApiName eq 'GetMessages'"</c>, the result will contain 2 time series.
        /// </summary>
        public IReadOnlyList<MetricTimeSeries> Timeseries { get; }
    }
}