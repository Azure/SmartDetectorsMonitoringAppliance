namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request for a smart signal execution
    /// </summary>
    public class SmartSignalRequest
    {
        /// <summary>
        /// Gets the resource IDs on which to run the signal
        /// </summary>
        public List<string> ResourceIds { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        public string SignalId { get; }

        /// <summary>
        /// Gets the end of the time range for analysis
        /// </summary>
        public DateTime AnalysisTimestamp { get; }

        /// <summary>
        /// Gets the analysis windows size (in minutes)
        /// </summary>
        public int AnalysisWindowSize { get; }

        /// <summary>
        /// Gets the analysis configuration
        /// </summary>
        public SmartSignalConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the signal</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="analysisTimestamp">The end of the time range for analysis</param>
        /// <param name="analysisWindowSize">The analysis windows size (in minutes)</param>
        /// <param name="configuration">The analysis configuration</param>
        public SmartSignalRequest(List<string> resourceIds, string signalId, DateTime analysisTimestamp, int analysisWindowSize, SmartSignalConfiguration configuration)
        {
            ResourceIds = resourceIds;
            SignalId = signalId;
            AnalysisTimestamp = analysisTimestamp;
            AnalysisWindowSize = analysisWindowSize;
            Configuration = configuration;
        }
    }
}
