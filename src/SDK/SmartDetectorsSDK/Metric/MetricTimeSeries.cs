//-----------------------------------------------------------------------
// <copyright file="MetricTimeSeries.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a time series of metric values
    /// </summary>
    public class MetricTimeSeries
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSeries"/> class
        /// </summary>
        /// <param name="data">The data points of metric values</param>
        /// <param name="metaData">The meta data regarding the list of data points</param>
        public MetricTimeSeries(IReadOnlyList<MetricValues> data, IReadOnlyList<KeyValuePair<string, string>> metaData)
        {
            this.Data = data;
            this.MetaData = metaData;
        }

        /// <summary>
        /// Gets the data points of metric values
        /// </summary>
        public IReadOnlyList<MetricValues> Data { get; }

        /// <summary>
        /// Gets the meta data regarding the list of data points
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> MetaData { get; }
    }
}