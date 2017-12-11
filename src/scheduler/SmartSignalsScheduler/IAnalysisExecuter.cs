namespace Microsoft.SmartSignals.Scheduler
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
        /// Executes the signal via the analysis flow
        /// </summary>
        /// <param name="signalExecutionInfo">The signal execution information</param>
        /// <param name="resourceIds">The resource IDs used by the signal</param>
        /// <returns>The signal detections</returns>
        Task<IList<SmartSignalDetection>> ExecuteSignalAsync(SignalExecutionInfo signalExecutionInfo, IList<string> resourceIds);
    }
}
