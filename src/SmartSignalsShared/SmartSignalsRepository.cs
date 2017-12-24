//-----------------------------------------------------------------------
// <copyright file="SmartSignalsRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalsRepository"/> interface
    /// </summary>
    public class SmartSignalsRepository : ISmartSignalsRepository
    {
        /// <summary>
        /// Reads a smart signal's metadata from the repository
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signal metadata</returns>
        public Task<SmartSignalMetadata> ReadSignalMetadataAsync(string signalId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Reads a smart signal's assemblies from the repository
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning a dictionary, mapping an assembly name to the assembly bytes</returns>
        public Task<Dictionary<string, byte[]>> ReadSignalAssembliesAsync(string signalId)
        {
            throw new System.NotImplementedException();
        }
    }
}