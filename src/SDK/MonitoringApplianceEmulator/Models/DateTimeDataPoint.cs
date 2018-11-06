//-----------------------------------------------------------------------
// <copyright file="DateTimeDataPoint.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;

    /// <summary>
    /// Represents a <see cref="DateTime"/> chart data point.
    /// </summary>
    public class DateTimeDataPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeDataPoint"/> class.
        /// </summary>
        /// <param name="dateTime">The point's date time</param>
        /// <param name="value">The point's value</param>
        public DateTimeDataPoint(DateTime dateTime, double value)
        {
            this.DateTime = dateTime;
            this.Value = value;
        }

        /// <summary>
        /// Gets the point's date time.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets the point's value.
        /// </summary>
        public double Value { get; }
    }
}
