//-----------------------------------------------------------------------
// <copyright file="PageableLogArchive.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.ObjectModel;
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
        private readonly string archiveFileName;

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

            // Set the archive file name
            this.archiveFileName = Path.Combine(archiveFolderName, "logs.zip");

            // And read the current log names
            using (ZipArchive logZipArchive = ZipFile.OpenRead(this.archiveFileName))
            {
                this.LogNames = new ObservableCollection<string>(logZipArchive.Entries.Select(entry => entry.Name));
            }
        }

        #region Implementation of IPageableLogArchive

        /// <summary>
        /// Gets the names of logs in this archive
        /// </summary>
        public ObservableCollection<string> LogNames { get; }

        /// <summary>
        /// Returns the requested log from the archive. If the log does not exist, a new empty log will be created and returned.
        /// </summary>
        /// <param name="logName">The name of the requested log.</param>
        /// <param name="initialPageSize">The log's initial page size</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation, returning the requested log.</returns>
        public async Task<IPageableLog> GetLogAsync(string logName, int initialPageSize)
        {
            IPageableLog log = await PageableLog.LoadLogFileAsync(this.archiveFileName, logName, initialPageSize);
            if (!this.LogNames.Contains(log.Name))
            {
                this.LogNames.Add(log.Name);
            }

            return log;
        }

        #endregion
    }
}
