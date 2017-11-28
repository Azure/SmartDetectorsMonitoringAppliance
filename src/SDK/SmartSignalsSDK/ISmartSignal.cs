//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The main interface to be implemented by Smart Signals.
    /// </summary>
    public interface ISmartSignal
    {
        /// <summary>
        /// Initiates an asynchronous operation for analyzing the smart signal on the specified resources.
        /// </summary>
        /// <param name="targetResources">A list of resource identifiers to analyze.</param>
        /// <param name="analysisWindow">
        /// A time range to perform the smart signal analysis on. Although a specific smart signal implementation may query telemetry from
        /// a wider time range in order to perform the analysis, the resulting detections must be manifested in this time window.
        /// </param>
        /// <param name="analysisServices">Contains the analysis services clients to be used for querying the resources telemetry.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the signal's execution. This telemetry will be used for troubleshooting and
        /// monitoring the signal's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation, returning the list of detections found on the target resources. If
        /// the list is <code>null</code> or empty, then it is assumed that no issues were detected.
        /// </returns>
        Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(
            List<ResourceIdentifier> targetResources,
            TimeRange analysisWindow,
            ISmartSignalAnalysisServices analysisServices,
            ITracer tracer,
            CancellationToken cancellationToken);
    }
}
