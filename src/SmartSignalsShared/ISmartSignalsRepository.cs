//-----------------------------------------------------------------------
// <copyright file="ISmartSignalsRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the smart signals repository
    /// </summary>
    public interface ISmartSignalsRepository
    {
        /// <summary>
        /// Reads all the smart signals metadata from the repository
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signals metadata</returns>
        Task<IList<SmartSignalMetadata>> ReadAllSignalsMetadataAsync();

        /// <summary>
        /// Reads a smart signal's metadata from the repository
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signal metadata</returns>
        Task<SmartSignalMetadata> ReadSignalMetadataAsync(string signalId);

        /// <summary>
        /// Reads a smart signal's assemblies from the repository
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning a dictionary, mapping an assembly name to the assembly bytes</returns>
        Task<Dictionary<string, byte[]>> ReadSignalAssembliesAsync(string signalId);
    }
}