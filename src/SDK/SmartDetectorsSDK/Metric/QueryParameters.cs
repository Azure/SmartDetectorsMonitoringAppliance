//-----------------------------------------------------------------------
// <copyright file="QueryParameters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Metric query properties to be used when fetching metric data. All fields are optional.
    /// <see href="https://docs.microsoft.com/en-us/powershell/module/azurerm.insights/get-azurermmetricdefinition?view=azurermps-5.4.0">Use Get-AzureRmMetricDefinition, to fetch for available metric names, granularity, etc.</see>
    /// </summary>
    public class QueryParameters
    {
        /// <summary>
        /// Gets or sets the start time of the time range
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the time range
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets a string representation of the timespan from which we want to fetch data.
        /// </summary>
        public string TimeRange
        {
            get
            {
                const string TimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

                string timespan = null;
                if (this.StartTime.HasValue && this.EndTime.HasValue)
                {
                    timespan = $"{this.StartTime.Value.ToString(TimeFormat, CultureInfo.InvariantCulture)}/{this.EndTime.Value.ToString(TimeFormat, CultureInfo.InvariantCulture)}";
                }

                return timespan;
            }
        }

        /// <summary>
        /// Gets or sets the resolution\ granularity of the results
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        /// Gets or sets the names of the resource metrics to be fetched.
        /// The metric names are part of the metric definitions, which can be retrieved using the metric client.
        /// For example, metric names for azure storage queues include: QueueMessageCount, QueueCapacity, QueueCount, QueueMessageCount, Transactions, Ingress, Egress, etc.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is only used as input to the GetResourceMetricsAsync, and is a DTO by nature")]
        public List<string> MetricNames { get; set; }

        /// <summary>
        /// Gets or sets the metric namespace of the metrics to be fetched.
        /// </summary>
        public string MetricNamespace { get; set; }

        /// <summary>
        /// Gets or sets the data aggregation types to perform on the fetched data
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is only used as input to the GetResourceMetricsAsync, and is a DTO by nature")]
        public List<Aggregation> Aggregations { get; set; }

        /// <summary>
        /// Gets or sets the field to order the fetched results by
        /// </summary>
        public string Orderby { get; set; }

        /// <summary>
        /// Gets or sets the amount of results to be fetched
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Gets or sets the filter to be used, based on the metric's dimensions.
        /// E.g. for Queues: <c>"ApiName eq 'GetMessage' or ApiName eq 'GetMessages'"</c>
        /// The result for each dimension will be returned in a different TimeSeries.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Returns a string that represents the metric query parameters.
        /// </summary>
        /// <returns>A string that represents the metric query parameters</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
