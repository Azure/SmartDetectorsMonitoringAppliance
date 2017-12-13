namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using NCrontab;

    /// <summary>
    /// Holds a smart signal configuration
    /// </summary>
    public class SmartSignalConfiguration
    {
        /// <summary>
        /// Gets or sets the signal ID.
        /// </summary>
        public string SignalId { get; set; }

        /// <summary>
        /// Gets or sets the resource type supported by the signal.
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the signal's schedule.
        /// </summary>
        public CrontabSchedule Schedule { get; set; }
    }
}