//-----------------------------------------------------------------------
// <copyright file="ChartType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum represents a type of a chart
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartType
    {
        /// <summary>
        /// Represents a line chart
        /// </summary>
        LineChart,

        /// <summary>
        /// Represents a bar chart
        /// </summary>
        BarChart
    }
}
