//-----------------------------------------------------------------------
// <copyright file="DynamicThresholdFailingPeriodsSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// Failing periods settings for dynamic threshold.
    /// </summary>
    public class DynamicThresholdFailingPeriodsSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicThresholdFailingPeriodsSettings"/> class.
        /// </summary>
        /// <param name="consecutivePeriods">The number of consecutive periods to evaluate.</param>
        /// <param name="consecutiveViolations">The minimal number of failing periods to be considered as threshold violation.</param>
        public DynamicThresholdFailingPeriodsSettings(uint consecutivePeriods, uint consecutiveViolations)
        {
            this.ConsecutivePeriods = consecutivePeriods;
            this.ConsecutiveViolations = consecutiveViolations;
        }

        /// <summary>
        /// Gets the number of consecutive periods to evaluate.
        /// </summary>
        public uint ConsecutivePeriods { get; }

        /// <summary>
        /// Gets the minimal number of failing periods to be considered as threshold violation.
        /// </summary>
        public uint ConsecutiveViolations { get; }
    }
}
