//-----------------------------------------------------------------------
// <copyright file="ISmartDetector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface to be implemented by Smart Detectors.
    /// </summary>
    public interface ISmartDetector
    {
        /// <summary>
        /// Initiates an asynchronous operation for running the Smart Detector analysis on the specified resources.
        /// </summary>
        /// <param name="analysisRequest">The analysis request data.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the Smart Detector's execution. This telemetry will be used for troubleshooting and
        /// monitoring the Smart Detector's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation, returning the Alerts detected for the target resources.
        /// </returns>
        Task<List<Alert>> AnalyzeResourcesAsync(
            AnalysisRequest analysisRequest,
            ITracer tracer,
            CancellationToken cancellationToken);
    }
}
