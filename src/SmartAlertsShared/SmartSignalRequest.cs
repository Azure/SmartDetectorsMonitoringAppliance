namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
{
    using System;
    using System.Collections.Generic;

    public class SmartSignalRequest
    {
        public List<string> ResourceIds { get; set; }

        public string SignalId { get; set; }

        public DateTime AnalysisTimestamp { get; set; }

        public int AnalysisWindowSize { get; set; }

        public SmartSignalConfiguration Configuration { get; set; }
    }
}
