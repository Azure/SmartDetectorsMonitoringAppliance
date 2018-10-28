//-----------------------------------------------------------------------
// <copyright file="PageableLogArchive.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// An implementation of the <see cref="IPageableLogArchive"/> interface, which
    /// stores the logs in a zip archive
    /// </summary>
    public sealed class PageableLogArchive : IPageableLogArchive
    {
        private ZipArchive logZipArchive;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageableLogArchive"/> class.
        /// </summary>
        public PageableLogArchive()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartAlertsEmulator"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageableLogArchive"/> class.
        /// </summary>
        /// <param name="archiveFolderName">The name of the folder to store the logs archive in.</param>
        public PageableLogArchive(string archiveFolderName)
        {
            if (!Directory.Exists(archiveFolderName))
            {
                Directory.CreateDirectory(archiveFolderName);
            }

            // Set the archive file name, and make sure it exists
            string archiveFileName = Path.Combine(archiveFolderName, "logs.zip");
            if (!File.Exists(archiveFileName))
            {
                using (ZipFile.Open(archiveFileName, ZipArchiveMode.Create))
                {
                }
            }

            // And open it for update
            this.logZipArchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
        }

        #region Implementation of IPageableLogArchive

        /// <summary>
        /// Returns the names of log files in the archive.
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation, returning the names of log files in the archive.</returns>
        public Task<List<string>> GetLogNamesAsync()
        {
            return Task.FromResult(this.logZipArchive.Entries.Select(entry => entry.Name).ToList());
        }

        /// <summary>
        /// Returns the requested log from the archive. If the log does not exist, a new empty log will be created and returned.
        /// </summary>
        /// <param name="logName">The name of the requested log.</param>
        /// <param name="initialPageSize">The log's initial page size</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation, returning the requested log.</returns>
        public async Task<IPageableLogTracer> GetLogAsync(string logName, int initialPageSize)
        {
            ZipArchiveEntry archiveEntry = await Task.Run(() =>
            {
                ZipArchiveEntry requestedEntry = this.logZipArchive.Entries.FirstOrDefault(entry => entry.Name == logName) ??
                                                 this.logZipArchive.CreateEntry(logName, CompressionLevel.Optimal);

                return requestedEntry;
            });

            return await PageableLogFileTracer.LoadLogFileAsync(archiveEntry, initialPageSize);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// There's no need to implement the full disposable pattern here since the class is <c>sealed</c>.
        /// </summary>
        public void Dispose()
        {
            this.logZipArchive?.Dispose();
            this.logZipArchive = null;
        }

        #endregion
    }
}
