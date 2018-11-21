//-----------------------------------------------------------------------
// <copyright file="IAutomaticResolutionSmartDetector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface to be implemented by Smart Detectors that support
    /// automatic resolution of Alerts.
    /// </summary>
    public interface IAutomaticResolutionSmartDetector : ISmartDetector
    {
        /// <summary>
        /// Checks if the alert indicated by <paramref name="automaticResolutionCheckRequest"/> should be
        /// automatically resolved.
        /// </summary>
        /// <param name="automaticResolutionCheckRequest">The automatic resolution check request.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the Smart Detector's execution. This telemetry will be used for troubleshooting and
        /// monitoring the Smart Detector's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, returning the automatic resolution check response.</returns>
        Task<AutomaticResolutionCheckResponse> CheckForAutomaticResolutionAsync(
            AutomaticResolutionCheckRequest automaticResolutionCheckRequest,
            ITracer tracer,
            CancellationToken cancellationToken);
    }
}
