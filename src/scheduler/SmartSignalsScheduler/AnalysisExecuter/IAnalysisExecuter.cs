namespace Microsoft.SmartSignals.Scheduler.AnalysisExecuter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;

    /// <summary>
    /// An interface responsible for executing signals via the analysis flow
    /// </summary>
    public interface IAnalysisExecuter
    {
        /// <summary>
        /// Execute the signal via the analysis flow
        /// </summary>
        /// <param name="signalConfiguration">The signal configuration</param>
        /// <param name="resourceIds">The resources IDs used by the signal</param>
        /// <param name="lastAnalysisEndTime">the last analysis time of the signal</param>
        /// <returns>The signal detections</returns>
        Task<SmartSignalDetection> ExecuteSignalAsync(SmartSignalConfiguration signalConfiguration, IList<string> resourceIds, DateTime? lastAnalysisEndTime = null);
    }
}
