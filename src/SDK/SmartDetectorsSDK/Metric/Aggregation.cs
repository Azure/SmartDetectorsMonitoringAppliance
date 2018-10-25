//-----------------------------------------------------------------------
// <copyright file="Aggregation.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Metric aggregation types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Aggregation
    {
        /// <summary>
        /// No aggregation
        /// </summary>
        None = 0,

        /// <summary>
        /// Aggregation of type Average
        /// </summary>
        Average = 1,

        /// <summary>
        /// Aggregation of type Count
        /// </summary>
        Count = 2,

        /// <summary>
        /// Aggregation of type Minimum
        /// </summary>
        Minimum = 3,

        /// <summary>
        /// Aggregation of type Maximum
        /// </summary>
        Maximum = 4,

        /// <summary>
        /// Aggregation of type Total (Sum)
        /// </summary>
        Total = 5
    }
}
