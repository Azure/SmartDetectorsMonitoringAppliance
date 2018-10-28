//-----------------------------------------------------------------------
// <copyright file="IPageableLogArchive.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for implementing a pageable log archive
    /// </summary>
    public interface IPageableLogArchive : IDisposable
    {
        /// <summary>
        /// Returns the names of log files in the archive.
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation, returning the names of log files in the archive.</returns>
        Task<List<string>> GetLogNamesAsync();

        /// <summary>
        /// Returns the requested log from the archive. If the log does not exist, a new empty log will be created and returned.
        /// </summary>
        /// <param name="logName">The name of the requested log.</param>
        /// <param name="initialPageSize">The log's initial page size</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation, returning the requested log.</returns>
        Task<IPageableLogTracer> GetLogAsync(string logName, int initialPageSize);
    }
}
