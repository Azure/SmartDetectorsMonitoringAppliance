//-----------------------------------------------------------------------
// <copyright file="ResponseInnerExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;

    /// <summary>
    /// Extension methods for <see cref="ResponseInner"/>
    /// </summary>
    public static class ResponseInnerExtensions
    {
        /// <summary>
        /// Converts a <see cref="ResponseInner"/> to a list of <see cref="MetricQueryResult"/> and returns it
        /// </summary>
        /// <param name="queryResponse">The metric query response as returned by Azure Monitoring</param>
        /// <returns>A list of metric query results</returns>
        public static IList<MetricQueryResult> ToMetricQueryResult(this ResponseInner queryResponse)
        {
            var queryResults = new List<MetricQueryResult>();

            // Convert each metric (a single metric is created per metric name)
            foreach (Metric metric in queryResponse.Value)
            {
                List<MetricTimeSeries> timeSeriesList = new List<MetricTimeSeries>();

                if (metric.Timeseries != null)
                {
                    // Convert the time series. A time series is created per filtered dimension.
                    // The info regarding the relevant dimension is set int he MetaData field
                    foreach (TimeSeriesElement timeSeries in metric.Timeseries)
                    {
                        var data = new List<MetricValues>();
                        var metaData = new List<KeyValuePair<string, string>>();

                        if (timeSeries.Data != null)
                        {
                            // Convert all metric values
                            data = timeSeries.Data.Select(metricValue =>
                                new MetricValues(metricValue.TimeStamp, metricValue.Average, metricValue.Minimum, metricValue.Maximum, metricValue.Total, metricValue.Count)).ToList();
                        }

                        if (timeSeries.Metadatavalues != null)
                        {
                            // Convert metadata
                            metaData = timeSeries.Metadatavalues.Select(metaDataValue =>
                                new KeyValuePair<string, string>(metaDataValue.Name.Value, metaDataValue.Value)).ToList();
                        }

                        timeSeriesList.Add(new MetricTimeSeries(data, metaData));
                    }
                }

                var queryResult = new MetricQueryResult(metric.Name.Value, metric.Unit.ToString(), timeSeriesList);

                queryResults.Add(queryResult);
            }

            return queryResults;
        }
    }
}
