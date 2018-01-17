//-----------------------------------------------------------------------
// <copyright file="SmartSignalRequest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request for a smart signal execution
    /// </summary>
    public class SmartSignalRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the signal</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="lastExecutionTime">The last execution of the signal. This can be null if the signal never ran.</param>
        /// <param name="cadence">The signal configured cadence</param>
        /// <param name="settings">The analysis settings</param>
        public SmartSignalRequest(IList<string> resourceIds, string signalId, DateTime? lastExecutionTime, TimeSpan cadence, SmartSignalSettings settings)
        {
            this.ResourceIds = resourceIds;
            this.SignalId = signalId;
            this.LastExecutionTime = lastExecutionTime;
            this.Settings = settings;
            this.Cadence = cadence;
        }

        /// <summary>
        /// Gets the resource IDs on which to run the signal
        /// </summary>
        public IList<string> ResourceIds { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        public string SignalId { get; }

        /// <summary>
        /// Gets the last execution time
        /// </summary>
        public DateTime? LastExecutionTime { get; }

        /// <summary>
        /// Gets the signal configured cadence
        /// </summary>
        public TimeSpan Cadence { get; }

        /// <summary>
        /// Gets the analysis settings
        /// </summary>
        public SmartSignalSettings Settings { get; }
    }
}
