//-----------------------------------------------------------------------
// <copyright file="PageableLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator;

    /// <summary>
    /// An implementation of <see cref="IPageableLog"/> that stores the traces in a log archive.
    /// </summary>
    public sealed class PageableLog : ObservableObject, IPageableLog
    {
        private readonly object tracesLock = new object();
        private readonly List<TraceLine> allTraces;
        private int pageSize;
        private int currentPageIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageableLog"/> class.
        /// </summary>
        /// <param name="archiveFileName">The filename of the logs zip archive.</param>
        /// <param name="logName">The name of the log to load.</param>
        /// <param name="initialPageSize">The initial page size to use.</param>
        /// <param name="allTraces">The traces that were loaded from the log file.</param>
        private PageableLog(string archiveFileName, string logName, int initialPageSize, List<TraceLine> allTraces)
        {
            this.ArchiveFileName = archiveFileName;
            this.Name = logName;

            this.allTraces = allTraces;

            this.CurrentPageTraces = new ObservableCollection<TraceLine>(allTraces.Take(initialPageSize));
            this.pageSize = initialPageSize;
            this.currentPageIndex = 0;
        }

        #region Implementation of IPageableLog properties

        /// <summary>
        /// Gets the log name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the total number of rows in the log.
        /// </summary>
        public int NumberOfTraceLines => this.allTraces.Count;

        /// <summary>
        /// Gets or sets the page size (number of lines in a single page).
        /// </summary>
        public int PageSize
        {
            get => this.pageSize;
            set
            {
                // Make sure we need to update anything
                if (value == this.pageSize)
                {
                    return;
                }

                // Lets try to make sure the first line of the current page will also be included in the new current page
                int firstLineIndex = this.CurrentPageIndex * this.pageSize;

                // Update everything that needs to be updated
                this.pageSize = value;
                this.OnPropertyChanged(nameof(this.PageSize));
                this.OnPropertyChanged(nameof(this.NumberOfPages));

                // And load the new page
                this.CurrentPageIndex = firstLineIndex / value;
            }
        }

        /// <summary>
        /// Gets the total number of pages in the log - this can change when new trace
        /// lines are added, or when <see cref="IPageableLog.PageSize"/> is changed.
        /// </summary>
        public int NumberOfPages => (int)Math.Ceiling((double)this.NumberOfTraceLines / this.PageSize);

        /// <summary>
        /// Gets or sets the current page index.
        /// </summary>
        public int CurrentPageIndex
        {
            get => this.currentPageIndex;
            set
            {
                // Make sure we got a valid page index - ignore if not
                if (value < 0 || value >= this.NumberOfPages || (value == this.CurrentPageIndex && this.CurrentPageTraces.Count == this.PageSize))
                {
                    return;
                }

                lock (this.tracesLock)
                {
                    // Update the page index
                    this.currentPageIndex = value;
                    this.OnPropertyChanged(nameof(this.CurrentPageIndex));

                    // And take the traces
                    this.CurrentPageTraces.Clear();
                    int firstPageLine = value * this.PageSize;
                    for (int i = firstPageLine; i < Math.Min(firstPageLine + this.PageSize, this.allTraces.Count); i++)
                    {
                        this.CurrentPageTraces.Add(this.allTraces[i]);
                    }

                    // Update the page start and end
                    this.OnPropertyChanged(nameof(this.CurrentPageStart));
                    this.OnPropertyChanged(nameof(this.CurrentPageEnd));
                }
            }
        }

        /// <summary>
        /// Gets the index of the current page's first trace line.
        /// </summary>
        public int CurrentPageStart => this.CurrentPageIndex * this.PageSize;

        /// <summary>
        /// Gets the index of the current page's last trace line.
        /// </summary>
        public int CurrentPageEnd => this.CurrentPageTraces.Count == 0 ? 0 : this.CurrentPageStart + this.CurrentPageTraces.Count - 1;

        /// <summary>
        /// Gets the traces of the current page
        /// </summary>
        public ObservableCollection<TraceLine> CurrentPageTraces { get; }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the log archive file name
        /// </summary>
        public string ArchiveFileName { get; }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="PageableLog"/> class from the given zip archive entry.
        /// This method will read all trace lines in the log file to calculate the blocks and return a new instance with
        /// the first page already loaded.
        /// </summary>
        /// <param name="archiveFileName">The filename of the logs zip archive.</param>
        /// <param name="logName">The name of the log to load.</param>
        /// <param name="initialPageSize">The initial page size to use.</param>
        /// <returns>The newly created <see cref="PageableLog"/> object.</returns>
        public static async Task<PageableLog> LoadLogFileAsync(string archiveFileName, string logName, int initialPageSize)
        {
            // Open the archive for read
            using (var logZipArchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update))
            {
                ZipArchiveEntry logFileEntry = logZipArchive.Entries.FirstOrDefault(entry => entry.Name == logName);
                if (logFileEntry == null)
                {
                    logZipArchive.CreateEntry(logName, CompressionLevel.Optimal);
                    return new PageableLog(archiveFileName, logName, initialPageSize, new List<TraceLine>());
                }

                // Read the whole file - even for millions of trace lines it should be OK.
                var allTraces = new List<TraceLine>();
                using (var reader = new StreamReader(logFileEntry.Open()))
                {
                    while (!reader.EndOfStream)
                    {
                        string traceLine = await reader.ReadLineAsync();
                        allTraces.Add(TraceLine.Parse(traceLine));
                    }
                }

                return new PageableLog(archiveFileName, logName, initialPageSize, allTraces);
            }
        }

        #region Implementation of IPageableLog methods

        /// <summary>
        /// Creates a tracer that sends trace lines to the log.
        /// </summary>
        /// <returns>A tracer that sends trace lines to the log.</returns>
        public ILogArchiveTracer CreateTracer()
        {
            return new LogArchiveTracer(this);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a trace line to the log, and updates all relevant properties
        /// </summary>
        /// <param name="traceLine">The line to trace.</param>
        public void AddTraceLine(TraceLine traceLine)
        {
            // Test hook for supporting tracing in tests
            if (System.Windows.Application.Current == null)
            {
                this.AddTraceLineInternal(traceLine);
            }
            else
            {
                // Since traces are emitted from a thread which is not the UI thread, we must synchronize this
                // to the UI thread so things won't break
                System.Windows.Application.Current.Dispatcher.Invoke(() => this.AddTraceLineInternal(traceLine));
            }
        }

        #endregion

        /// <summary>
        /// Adds a trace line to the log, and updates all relevant properties
        /// </summary>
        /// <param name="traceLine">The line to trace.</param>
        private void AddTraceLineInternal(TraceLine traceLine)
        {
            lock (this.tracesLock)
            {
                this.allTraces.Add(traceLine);
                this.OnPropertyChanged(nameof(this.NumberOfTraceLines));

                // Check if we've just added a page to the log - if so, notify it and move
                // to show the last page
                if (this.NumberOfTraceLines % this.PageSize == 1)
                {
                    this.OnPropertyChanged(nameof(this.NumberOfPages));
                }

                // Final check if we have need to add the trace to the current page
                if (this.CurrentPageTraces.Count < this.PageSize)
                {
                    this.CurrentPageTraces.Add(traceLine);
                    this.OnPropertyChanged(nameof(this.CurrentPageEnd));
                }
                else
                {
                    // We've filled the current page, so move on to the next
                    this.CurrentPageIndex = this.NumberOfPages - 1;
                }
            }
        }
    }
}
