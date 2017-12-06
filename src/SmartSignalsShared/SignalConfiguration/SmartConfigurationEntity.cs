namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the smart signal running configuration.
    /// The signal ID is the row key.
    /// </summary>
    public class SmartConfigurationEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the type of the resource applicable for the signal.
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the signal's schedule
        /// </summary>
        public string CrontabSchedule { get; set; }
    }
}
