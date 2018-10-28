//-----------------------------------------------------------------------
// <copyright file="TraceLine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A class for holding a specific trace line
    /// </summary>
    public class TraceLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLine"/> class.
        /// </summary>
        /// <param name="level">The level of the trace line (info, verbose etc.).</param>
        /// <param name="timeStamp">The time stamp in which the trace line was emitted.</param>
        /// <param name="message">The trace line's message.</param>
        public TraceLine(TraceLevel level, DateTime timeStamp, string message)
        {
            this.Level = level;
            this.TimeStamp = timeStamp;
            this.Message = message;
        }

        /// <summary>
        /// Gets the level of the trace line (info, verbose etc.).
        /// </summary>
        public TraceLevel Level { get; }

        /// <summary>
        /// Gets the time stamp in which the trace line was emitted.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Gets the trace line's message.
        /// </summary>
        public string Message { get; }
    }
}
