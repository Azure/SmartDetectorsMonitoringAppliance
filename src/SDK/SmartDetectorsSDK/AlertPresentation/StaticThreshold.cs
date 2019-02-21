//-----------------------------------------------------------------------
// <copyright file="StaticThreshold.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// Represents a metric static threshold.
    /// </summary>
    public class StaticThreshold
    {
        /// <summary>
        /// Gets or sets the metric's upper threshold.
        /// </summary>
        public double UpperThreshold { get; set; }

        /// <summary>
        /// Gets or sets the metric's lower threshold.
        /// </summary>
        public double LowerThreshold { get; set; }
    }
}
