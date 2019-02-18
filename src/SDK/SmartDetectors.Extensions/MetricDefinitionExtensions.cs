//-----------------------------------------------------------------------
// <copyright file="MetricDefinitionExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using MetricDefinition = Microsoft.Azure.Monitoring.SmartDetectors.Metric.MetricDefinition;

    /// <summary>
    /// Extension methods for <see cref="Management.Monitor.Fluent.Models.MetricDefinition"/>
    /// </summary>
    public static class MetricDefinitionExtensions
    {
        /// <summary>
        /// Converts a <see cref="Management.Monitor.Fluent.Models.MetricDefinition"/> response to a <see cref="MetricDefinition"/> and returns it.
        /// </summary>
        /// <param name="definition">The metric definition</param>
        /// <returns>The conversion result</returns>
        public static MetricDefinition ConvertToSmartDetectorsMetricDefinition(this Management.Monitor.Fluent.Models.MetricDefinition definition)
        {
            return new MetricDefinition(
                definition.Name.Value,
                definition.Dimensions?.Select(x => x.Value).ToList(),
                definition.IsDimensionRequired,
                definition.MetricAvailabilities?.Select(x => Tuple.Create(x.Retention, x.TimeGrain)).ToList(),
                definition.Unit?.ToString(),
                definition.PrimaryAggregationType.HasValue ? (Aggregation?)Enum.Parse(typeof(Aggregation), definition.PrimaryAggregationType.ToString()) : null);
        }
    }
}
