//-----------------------------------------------------------------------
// <copyright file="ISmartDetectorRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// Interface for the Smart Detector repository
    /// </summary>
    public interface ISmartDetectorRepository
    {
        /// <summary>
        /// Reads all the Smart Detectors manifests from the repository
        /// For each Smart Detector we return the latest version's manifest.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detectors manifests</returns>
        Task<IList<SmartDetectorManifest>> ReadAllSmartDetectorsManifestsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reads a Smart Detector's package from the repository
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector's ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detector package</returns>
        Task<SmartDetectorPackage> ReadSmartDetectorPackageAsync(string smartDetectorId, CancellationToken cancellationToken);

        /// <summary>
        /// Reads a Smart Detector's manifest from the repository
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector's ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detector manifest</returns>
        Task<SmartDetectorManifest> ReadSmartDetectorManifestAsync(string smartDetectorId, CancellationToken cancellationToken);
    }
}