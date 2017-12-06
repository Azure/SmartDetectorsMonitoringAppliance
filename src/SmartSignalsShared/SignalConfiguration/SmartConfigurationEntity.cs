namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the smart signal running configuration.
    /// The signal ID is the row key.
    /// </summary>
    public class SmartConfigurationEntity : TableEntity
    {
        public ResourceType ResourceType { get; set; }

        public string CrontabSchedule { get; set; }
    }
}
