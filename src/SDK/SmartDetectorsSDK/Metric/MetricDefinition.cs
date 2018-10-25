//-----------------------------------------------------------------------
// <copyright file="MetricDefinition.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///  An object which represents the definition of a single metric.
    /// </summary>
    public class MetricDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricDefinition"/> class.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="dimensions">The metric dimensions</param>
        /// <param name="isDimensionRequired">A boolean indicating whether the dimension is required</param>
        /// <param name="availabilities">The metric's availabilities, as a pair of <see cref="TimeSpan"/> objects indicating the retention and time grain</param>
        /// <param name="unit">The metric's value unit (Seconds, bytes, etc.)</param>
        /// <param name="primaryAggregationType">The metric's primary aggregation type</param>
        public MetricDefinition(string name, IReadOnlyList<string> dimensions, bool? isDimensionRequired, IReadOnlyList<Tuple<TimeSpan?, TimeSpan?>> availabilities, string unit, Aggregation? primaryAggregationType)
        {
            this.Name = name;
            this.Dimensions = dimensions;
            this.IsDimensionRequired = isDimensionRequired;
            this.Availabilities = availabilities;
            this.Unit = unit;
            this.PrimaryAggregationType = primaryAggregationType;
        }

        /// <summary>
        /// Gets the metric name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the metric dimensions names
        /// </summary>
        public IReadOnlyList<string> Dimensions { get; }

        /// <summary>
        /// Gets a boolean indicating whether the dimension is required
        /// </summary>
        public bool? IsDimensionRequired { get; }

        /// <summary>
        /// Gets the metric's availabilities, as a pair of <see cref="TimeSpan"/> objects indicating the retention and time grain
        /// </summary>
        public IReadOnlyList<Tuple<TimeSpan?, TimeSpan?>> Availabilities { get; }

        /// <summary>
        /// Gets the metric's value unit (Seconds, bytes, etc.)
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// Gets the metric's primary aggregation type
        /// </summary>
        public Aggregation? PrimaryAggregationType { get; }

        /// <summary>
        /// Returns the metric definition name.
        /// </summary>
        /// <returns>The metric definition name.</returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}