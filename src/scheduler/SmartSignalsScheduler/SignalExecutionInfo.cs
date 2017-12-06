namespace Microsoft.SmartSignals.Scheduler
{
    using System;

    /// <summary>
    /// Signal execution information
    /// </summary>
    public class SignalExecutionInfo
    {
        /// <summary>
        /// Gets or sets the signal ID
        /// </summary>
        public string SignalId { get; set; }

        /// <summary>
        /// Gets or sets the start time of the signal analysis windows
        /// </summary>
        public DateTime AnalysisStartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the signal analysis windows
        /// </summary>
        public DateTime AnalysisEndTime { get; set; }
    }
}
