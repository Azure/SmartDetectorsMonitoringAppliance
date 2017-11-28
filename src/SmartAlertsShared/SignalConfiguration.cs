namespace Microsoft.SmartAlerts.Shared
{
    using NCrontab;

    public class SignalConfiguration
    {
        public string SignalId { get; set; }

        public ResourceType ResourceType { get; set; }

        public CrontabSchedule Schedule { get; set; }
    }
}