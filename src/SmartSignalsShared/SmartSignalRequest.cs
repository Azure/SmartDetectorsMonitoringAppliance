namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request for a smart signal execution
    /// </summary>
    public class SmartSignalRequest
    {
        /// <summary>
        /// Gets the resource IDs on which to run the signal
        /// </summary>
        public IList<string> ResourceIds { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        public string SignalId { get; }

        /// <summary>
        /// Gets the start of the time range for analysis
        /// </summary>
        public DateTime AnalysisStartTime { get; }

        /// <summary>
        /// Gets the end time of the analysis
        /// </summary>
        public DateTime AnalysisEndTime { get; }

        /// <summary>
        /// Gets the analysis settings
        /// </summary>
        public SmartSignalSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the signal</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="analysisStartTime">The start of the time range for analysis</param>
        /// <param name="analysisEndTime">The end of the time range for analysis</param>
        /// <param name="settings">The analysis settings</param>
        public SmartSignalRequest(IList<string> resourceIds, string signalId, DateTime analysisStartTime, DateTime analysisEndTime, SmartSignalSettings settings)
        {
            ResourceIds = resourceIds;
            SignalId = signalId;
            AnalysisStartTime = analysisStartTime;
            AnalysisEndTime = analysisEndTime;
            Settings = settings;
        }
    }
}
