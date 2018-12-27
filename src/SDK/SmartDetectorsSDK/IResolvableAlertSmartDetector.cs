//-----------------------------------------------------------------------
// <copyright file="IResolvableAlertSmartDetector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface to be implemented by Smart Detectors that support resolution of Alerts.
    /// </summary>
    public interface IResolvableAlertSmartDetector : ISmartDetector
    {
        /// <summary>
        /// Checks if the alert indicated by <paramref name="alertResolutionCheckRequest"/> should be resolved.
        /// </summary>
        /// <param name="alertResolutionCheckRequest">The alert resolution check request.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the Smart Detector's execution. This telemetry will be used for troubleshooting and
        /// monitoring the Smart Detector's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, returning the alert resolution check response.</returns>
        Task<AlertResolutionCheckResponse> CheckForResolutionAsync(
            AlertResolutionCheckRequest alertResolutionCheckRequest,
            ITracer tracer,
            CancellationToken cancellationToken);
    }
}
