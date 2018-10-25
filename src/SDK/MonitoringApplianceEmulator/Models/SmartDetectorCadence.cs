//-----------------------------------------------------------------------
// <copyright file="SmartDetectorCadence.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Extensions;

    /// <summary>
    /// Represents a time cadence for a Smart Detector run.
    /// </summary>
    public class SmartDetectorCadence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorCadence"/> class.
        /// </summary>
        /// <param name="timeSpan">The cadence time span</param>
        public SmartDetectorCadence(TimeSpan timeSpan)
        {
            this.TimeSpan = timeSpan;
            this.DisplayName = timeSpan.ToReadableString();
        }

        /// <summary>
        /// Gets the cadence time span.
        /// </summary>
        public TimeSpan TimeSpan { get; }

        /// <summary>
        /// Gets the cadence display name.
        /// </summary>
        public string DisplayName { get; }
    }
}
