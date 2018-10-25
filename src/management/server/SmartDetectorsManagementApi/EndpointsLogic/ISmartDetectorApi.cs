//-----------------------------------------------------------------------
// <copyright file="ISmartDetectorApi.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// This interface represents the /smartDetector API logic.
    /// </summary>
    public interface ISmartDetectorApi
    {
        /// <summary>
        /// Gets all the Smart Detectors.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Detectors.</returns>
        /// <exception cref="SmartDetectorsManagementApiException">This exception is thrown when we failed to retrieve Smart Detectors.</exception>
        Task<ListSmartDetectorsResponse> GetSmartDetectorsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a Smart Detector.
        /// </summary>
        /// <param name="detectorId">The detector ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Detector.</returns>
        /// <exception cref="SmartDetectorsManagementApiException">This exception is thrown when we failed to retrieve the Smart Detector.</exception>
        Task<SmartDetector> GetSmartDetectorAsync(string detectorId, CancellationToken cancellationToken);
    }
}
