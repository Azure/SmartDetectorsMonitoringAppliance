namespace Microsoft.SmartSignals.Scheduler
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the last successful run of a singal job.
    /// The signal ID is the row key.
    /// </summary>
    public class TrackSignalRunEntity : TableEntity
    {
        /// <summary>
        /// The position of the last successful run
        /// </summary>
        public DateTime LastRunTime { get; set; }
    }
}
