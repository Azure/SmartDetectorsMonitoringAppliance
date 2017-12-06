using System;

namespace Microsoft.SmartSignals.Scheduler
{
    /// <summary>
    /// Signal execution information
    /// </summary>
    public class SignalExecutionInfo
    {
        /// <summary>
        /// The signal ID
        /// </summary>
        public string SignalId { get; set; }

        /// <summary>
        /// The start time of the signal analysis windows
        /// </summary>
        public DateTime AnalysisStartTime { get; set; }

        /// <summary>
        /// The end time of the signal analysis windows
        /// </summary>
        public DateTime AnalysisEndTime { get; set; }
    }
}
