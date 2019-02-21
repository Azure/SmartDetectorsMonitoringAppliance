//-----------------------------------------------------------------------
// <copyright file="DynamicThreshold.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// Represents a metric dynamic threshold.
    /// </summary>
    public class DynamicThreshold
    {
        /// <summary>
        /// The value for high sensitivity dynamic threshold.
        /// </summary>
        public const double HighSensitivity = 0;

        /// <summary>
        /// The value for medium sensitivity dynamic threshold.
        /// </summary>
        public const double MediumSensitivity = 1.5;

        /// <summary>
        /// The value for low sensitivity dynamic threshold.
        /// </summary>
        public const double LowSensitivity = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicThreshold"/> class.
        /// </summary>
        /// <param name="failingPeriodsSettings">The threshold's failing periods settings.</param>
        /// <param name="sensitivity">The thresholds sensitivity.</param>
        public DynamicThreshold(DynamicThresholdFailingPeriodsSettings failingPeriodsSettings, double sensitivity)
        {
            if (sensitivity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sensitivity), "Dynamic threshold sensitivity cannot have a negative value");
            }

            this.FailingPeriodsSettings = failingPeriodsSettings ?? throw new ArgumentNullException(nameof(failingPeriodsSettings));
            this.Sensitivity = sensitivity;
        }

        /// <summary>
        /// Gets the failing periods settings for the dynamic threshold.
        /// </summary>
        public DynamicThresholdFailingPeriodsSettings FailingPeriodsSettings { get; }

        /// <summary>
        /// Gets the dynamic threshold's sensitivity. This must be a non-negative
        /// number, and lower values indicate higher sensitivity.
        /// </summary>
        public double Sensitivity { get; }

        /// <summary>
        /// Gets or sets an optional date before which the dynamic threshold ML model won’t be trained on data.
        /// </summary>
        public DateTime? IgnoreDataBefore { get; set; }
    }
}
