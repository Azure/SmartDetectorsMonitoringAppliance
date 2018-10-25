//-----------------------------------------------------------------------
// <copyright file="AlertPresentationChartAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    /// <summary>
    /// An attribute defining the presentation of a chart property in an <see cref="Alert"/>.
    /// </summary>
    public class AlertPresentationChartAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationChartAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value.</param>
        /// <param name="chartType">The type of the chart</param>
        /// <param name="xAxistype">The type of the X axis</param>
        /// <param name="yAxisType">The type of the Y axis</param>
        public AlertPresentationChartAttribute(
            string displayName,
            ChartType chartType,
            ChartAxisType xAxistype,
            ChartAxisType yAxisType)
            : base(displayName)
        {
            this.ChartType = chartType;
            this.XAxisType = xAxistype;
            this.YAxisType = yAxisType;
        }

        /// <summary>
        /// Gets the chart type
        /// </summary>
        public ChartType ChartType { get; }

        /// <summary>
        /// Gets the X axis type
        /// </summary>
        public ChartAxisType XAxisType { get; }

        /// <summary>
        /// Gets the Y axis type
        /// </summary>
        public ChartAxisType YAxisType { get; }
    }
}
