//-----------------------------------------------------------------------
// <copyright file="ISmartSignal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface to be implemented by Smart Signals.
    /// </summary>
    public interface ISmartSignal
    {
        /// <summary>
        /// Initiates an asynchronous operation for analyzing the smart signal on the specified resources.
        /// </summary>
        /// <param name="analysisRequest">The analysis request data.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the signal's execution. This telemetry will be used for troubleshooting and
        /// monitoring the signal's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation, returning the Signal result for the target resources. 
        /// </returns>
        Task<SmartSignalResult> AnalyzeResourcesAsync(
            AnalysisRequest analysisRequest,
            ITracer tracer,
            CancellationToken cancellationToken);
    }
}
