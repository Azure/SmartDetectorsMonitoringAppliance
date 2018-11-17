//-----------------------------------------------------------------------
// <copyright file="ChartAxisType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum represents the type of a chart's axis
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartAxisType
    {
        /// <summary>
        /// Represents an axis of string labels
        /// </summary>
        StringAxis,

        /// <summary>
        /// Represents an axis of date labels
        /// </summary>
        DateAxis,

        /// <summary>
        /// Represents an axis of numeric labels
        /// </summary>
        NumberAxis
    }
}
