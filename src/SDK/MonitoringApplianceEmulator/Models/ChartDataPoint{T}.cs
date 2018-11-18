//-----------------------------------------------------------------------
// <copyright file="ChartDataPoint{T}.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    /// <summary>
    /// Represents a chart data point.
    /// </summary>
    /// <typeparam name="T">Type of X axis value.</typeparam>
    public class ChartDataPoint<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataPoint{T}"/> class.
        /// </summary>
        /// <param name="x">The point's X value</param>
        /// <param name="y">The point's Y value</param>
        public ChartDataPoint(T x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets the point's X value.
        /// </summary>
        public T X { get; }

        /// <summary>
        /// Gets the point's Y value.
        /// </summary>
        public double Y { get; }
    }
}
