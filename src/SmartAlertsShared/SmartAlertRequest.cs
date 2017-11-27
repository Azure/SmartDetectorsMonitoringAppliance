namespace Microsoft.SmartAlerts.Shared
{
    using System;
    using System.Collections.Generic;

    public class SmartAlertRequest
    {
        public List<string> ResourceIds { get; set; }

        public string SignalId { get; set; }

        public DateTime AnalysisTimestamp { get; set; }

        public int AnalysisWindowSize { get; set; }

        public SignalConfiguration Configuration { get; set; }
    }
}
