//-----------------------------------------------------------------------
// <copyright file="PageableLogFileTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// An implementation of <see cref="IPageableLogTracer"/> that stores the traces in a <see cref="ZipArchiveEntry"/>.
    /// </summary>
    public sealed class PageableLogFileTracer : ObservableObject, IPageableLogTracer
    {
        /// <summary>
        /// The block size used for analyzing the log file
        /// </summary>
        private const int BlockSize = 1000;

        private readonly SemaphoreSlim logFileLock = new SemaphoreSlim(1, 1);
        private readonly List<long> logBlocks;
        private Stream logFileStream;
        private StreamWriter tracerStreamWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageableLogFileTracer"/> class.
        /// </summary>
        /// <param name="logFileEntry">The zip archive entry containing the log's traces.</param>
        /// <param name="initialPageSize">The initial page size to use.</param>
        /// <param name="numberOfTraceLines">The total number of trace lines in the log file.</param>
        /// <param name="firstPageTraces">The traces of the first log's first page/</param>
        /// <param name="logBlocks">The pre-calculated list of log blocks positions.</param>
        private PageableLogFileTracer(ZipArchiveEntry logFileEntry, int initialPageSize, int numberOfTraceLines, List<TraceLine> firstPageTraces, List<long> logBlocks)
        {
            this.logBlocks = logBlocks;
            this.logFileStream = logFileEntry.Open();
            this.tracerStreamWriter = new StreamWriter(this.logFileStream);

            this.NumberOfTraceLines = numberOfTraceLines;
            this.PageSize = initialPageSize;
            this.CurrentPageIndex = 0;
            this.CurrentPageTraces = new ObservableCollection<TraceLine>(firstPageTraces);

            this.SessionId = logFileEntry.Name;
        }

        #region Implementation of IPageableLogTracer properties

        /// <summary>
        /// Gets the total number of rows in the log.
        /// </summary>
        public int NumberOfTraceLines { get; private set; }

        /// <summary>
        /// Gets the page size (number of lines in a single page).
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the total number of pages in the log - this can change when new trace
        /// lines are added, or when <see cref="IPageableLogTracer.PageSize"/> is changed.
        /// </summary>
        public int NumberOfPages => (int)Math.Ceiling((double)this.NumberOfTraceLines / this.PageSize);

        /// <summary>
        /// Gets the current page index.
        /// </summary>
        public int CurrentPageIndex { get; private set; }

        /// <summary>
        /// Gets the traces of the current page
        /// </summary>
        public ObservableCollection<TraceLine> CurrentPageTraces { get; }

        #endregion

        #region Implementation of ITracer properties

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId { get; }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="PageableLogFileTracer"/> class from the given zip archive entry.
        /// This method will read all trace lines in the log file to calculate the blocks and return a new instance with
        /// the first page already loaded.
        /// </summary>
        /// <param name="logFileEntry">The zip archive entry containing the log's traces.</param>
        /// <param name="initialPageSize">The initial page size to use.</param>
        /// <returns>The newly created <see cref="PageableLogFileTracer"/> object.</returns>
        public static async Task<PageableLogFileTracer> LoadLogFileAsync(ZipArchiveEntry logFileEntry, int initialPageSize)
        {
            Diagnostics.EnsureArgumentInRange(() => initialPageSize, 1, int.MaxValue);
            Diagnostics.EnsureArgumentNotNull(() => logFileEntry);

            // Read the whole file, and store its blocks. Each element in the blocks list contains the position in the
            // log file in which the block starts.
            int numberOfTraceLines = 0;
            var logBlocks = new List<long> { 0 };
            var firstPageTraces = new List<TraceLine>();
            using (var reader = new StreamReader(logFileEntry.Open()))
            {
                int currentBlockSize = 0;
                while (!reader.EndOfStream)
                {
                    string traceLine = await reader.ReadLineAsync();
                    numberOfTraceLines++;
                    currentBlockSize++;

                    if (firstPageTraces.Count < initialPageSize)
                    {
                        firstPageTraces.Add(ParseTraceLine(traceLine));
                    }

                    if (currentBlockSize == BlockSize)
                    {
                        logBlocks.Add(reader.BaseStream.Position);
                    }
                }
            }

            return new PageableLogFileTracer(logFileEntry, initialPageSize, numberOfTraceLines, firstPageTraces, logBlocks);
        }

        #region Implementation of IPageableLogTracer methods

        /// <summary>
        /// Sets the log's page size to be <paramref name="pageSize"/>. Calling this method will
        /// have the effect of an update to <see cref="CurrentPageTraces"/> and possibly to <see cref="CurrentPageIndex"/>.
        /// </summary>
        /// <param name="pageSize">The updated page size.</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task SetPageSizeAsync(int pageSize)
        {
            // Make sure we need to update anything
            if (pageSize == this.PageSize)
            {
                return;
            }

            // Lets try to make sure the first line of the current page will also be included in the new current page
            int firstLineIndex = this.CurrentPageIndex * this.PageSize;
            int newCurrentPageIndex = firstLineIndex / pageSize;

            // Update everything that needs to be updated
            this.PageSize = pageSize;
            this.OnPropertyChanged(nameof(this.PageSize));
            this.OnPropertyChanged(nameof(this.NumberOfPages));

            // And load the new page (we set CurrentPageIndex to -1 to force loading of the page)
            this.CurrentPageIndex = -1;
            await this.SetCurrentPageIndexAsync(newCurrentPageIndex);
        }

        /// <summary>
        /// Moves <see cref="CurrentPageIndex"/> to be the next page. If <see cref="CurrentPageIndex"/> already points
        /// to the log's last page, calling this method will not have any affect.
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task NextPageAsync()
        {
            await this.SetCurrentPageIndexAsync(this.CurrentPageIndex + 1);
        }

        /// <summary>
        /// Moves <see cref="CurrentPageIndex"/> to be the previous page. If <see cref="CurrentPageIndex"/> already points
        /// to the log's first page, calling this method will not have any affect.
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task PrevPageAsync()
        {
            await this.SetCurrentPageIndexAsync(this.CurrentPageIndex - 1);
        }

        /// <summary>
        /// Moves <see cref="CurrentPageIndex"/> to be <paramref name="pageIndex"/>. If <paramref name="pageIndex"/> is outside
        /// of the log's page range, calling this method will not have any affect.
        /// </summary>
        /// <param name="pageIndex">The page index to set.</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task SetCurrentPageIndexAsync(int pageIndex)
        {
            // Make sure we got a valid page index - ignore if not
            if (pageIndex < 0 || pageIndex >= this.NumberOfPages || pageIndex == this.CurrentPageIndex)
            {
                return;
            }

            // Update the page index
            this.CurrentPageIndex = pageIndex;
            this.OnPropertyChanged(nameof(this.CurrentPageIndex));

            // And read all the data
            this.CurrentPageTraces.Clear();

            await this.logFileLock.WaitAsync();
            try
            {
                // Skip to the first relevant block
                int firstBlock = pageIndex * this.PageSize / BlockSize;
                this.logFileStream.Position = this.logBlocks[firstBlock];

                using (var reader = new StreamReader(this.logFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                {
                    // Skip all lines until we start the page
                    int firstPageLine = pageIndex * this.PageSize;
                    int currentLineIndex = firstBlock * BlockSize;
                    while (currentLineIndex < firstPageLine)
                    {
                        await reader.ReadLineAsync();
                        currentLineIndex++;
                    }

                    // And read the page
                    for (int i = 0; i < this.PageSize; i++)
                    {
                        this.CurrentPageTraces.Add(ParseTraceLine(await reader.ReadLineAsync()));
                    }
                }

                // Move back to the end of the file so we won't override traces
                this.logFileStream.Seek(0, SeekOrigin.End);
            }
            finally
            {
                this.logFileLock.Release();
            }
        }

        #endregion

        #region Implementation of ITracer methods

        /// <summary>
        /// Trace <paramref name="message"/> as Information message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceInformation(string message)
        {
            this.Trace(TraceLevel.Info, message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceError(string message)
        {
            this.Trace(TraceLevel.Error, message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceVerbose(string message)
        {
            this.Trace(TraceLevel.Verbose, message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceWarning(string message)
        {
            this.Trace(TraceLevel.Warning, message);
        }

        /// <summary>
        /// Adds a custom property, to be included in all traces.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void AddCustomProperty(string name, string value)
        {
        }

        /// <summary>
        /// Gets the custom properties that are set in this tracer instance.
        /// </summary>
        /// <returns>The custom properties, as a read-only dictionary</returns>
        public IReadOnlyDictionary<string, string> GetCustomProperties()
        {
            return new Dictionary<string, string>();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// There's no need to implement the full disposable pattern here since the class is <c>sealed</c>.
        /// </summary>
        public void Dispose()
        {
            this.tracerStreamWriter?.Dispose();
            this.tracerStreamWriter = null;

            this.logFileStream?.Dispose();
            this.logFileStream = null;
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Parses <paramref name="traceLine"/> to a <see cref="TraceLine"/> object. The trace line is assumed to be
        /// of the same format we use to add trace lines to the log: {level}|{time stamp}|{message}
        /// </summary>
        /// <param name="traceLine">The trace line to parse.</param>
        /// <returns>The parsed trace line.</returns>
        private static TraceLine ParseTraceLine(string traceLine)
        {
            string[] lineParts = traceLine.Split('|');
            if (lineParts.Length < 3)
            {
                throw new InvalidOperationException($"Unable to parse trace line '{traceLine}'");
            }

            if (!Enum.TryParse(lineParts[0], out TraceLevel traceLevel))
            {
                throw new InvalidOperationException($"Invalid trace level for trace line '{traceLine}'");
            }

            if (!DateTime.TryParseExact(lineParts[1], "yyyy-MM-dd HH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime traceTimestamp))
            {
                throw new InvalidOperationException($"Invalid trace time stamp for trace line '{traceLine}'");
            }

            return new TraceLine(traceLevel, traceTimestamp, string.Join("|", lineParts.Skip(2)));
        }

        /// <summary>
        /// Trace <paramref name="message"/> with the specified level.
        /// </summary>
        /// <param name="level">The trace level.</param>
        /// <param name="message">The message to trace.</param>
        private void Trace(TraceLevel level, string message)
        {
            this.logFileLock.Wait();
            try
            {
                // Add the trace line to the log file
                DateTime timestamp = DateTime.UtcNow;
                this.tracerStreamWriter.WriteLine($"{level}|{timestamp:yyyy-MM-dd HH:mm:ss.fffZ}|{message}");
                this.tracerStreamWriter.Flush();

                // Update the total count
                this.NumberOfTraceLines++;
                this.OnPropertyChanged(nameof(this.NumberOfTraceLines));

                // Check if we've just added a page to the log - and just notify on it
                if (this.NumberOfTraceLines % this.PageSize == 1)
                {
                    this.OnPropertyChanged(nameof(this.NumberOfPages));
                }

                // Final check if we have need to add the trace to the current page
                if (this.CurrentPageTraces.Count < this.PageSize)
                {
                    this.CurrentPageTraces.Add(new TraceLine(level, timestamp, message));
                }
            }
            finally
            {
                this.logFileLock.Release();
            }
        }

        #endregion
    }
}
