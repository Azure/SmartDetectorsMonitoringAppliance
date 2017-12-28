//-----------------------------------------------------------------------
// <copyright file="ISmartSignalRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the Smart Signal repository
    /// </summary>
    public interface ISmartSignalRepository
    {
        /// <summary>
        /// Reads all the smart signals manifests from the repository
        /// For each signal we return the latest version's manifest.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signals manifests</returns>
        Task<IList<SmartSignalManifest>> ReadAllSignalsManifestsAsync();

        /// <summary>
        /// Reads a smart signal's package from the repository
        /// </summary>
        /// <param name="signalId">The signal's ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the signal package</returns>
        Task<SmartSignalPackage> ReadSignalPackageAsync(string signalId);
    }
}