namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AlertRules
{
    using NCrontab;

    /// <summary>
    /// Holds an alert rule
    /// </summary>
    public class AlertRule
    {
        /// <summary>
        /// Gets or sets the rule ID.
        /// </summary>
        public string Id { get; set; }

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