namespace Microsoft.SmartSignals.Scheduler
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the last successful run of a signal job.
    /// The signal ID is the row key.
    /// </summary>
    public class TrackSignalRunEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the start time of the last successful run
        /// </summary>
        public DateTime LastSuccessfulRunStartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the last successful run
        /// </summary>
        public DateTime LastSuccessfulRunEndTime { get; set; }
    }
}
