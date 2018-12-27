//-----------------------------------------------------------------------
// <copyright file="ChartPoint.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// A class represents a point in a chart
    /// </summary>
    public class ChartPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartPoint"/> class.
        /// </summary>
        /// <param name="x">The X value of the chart point</param>
        /// <param name="y">The Y value of the chart point</param>
        public ChartPoint(object x, object y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or sets the X value of the point
        /// </summary>
        public object X { get; set; }

        /// <summary>
        /// Gets or sets the Y value of the point
        /// </summary>
        public object Y { get; set; }
    }
}
