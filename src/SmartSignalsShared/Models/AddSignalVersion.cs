namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Models
{
    /// <summary>
    /// This class represents the model of the PUT signals request body.
    /// </summary>
    public class AddSignalVersion
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
        /// Gets or sets the scheduling configuration (in CRON format).
        /// </summary>
        public string Schedule { get; set; }
    }
}
