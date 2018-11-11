//-----------------------------------------------------------------------
// <copyright file="LogArchiveTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    /// An implementation of the <see cref="ILogArchiveTracer"/> interface which saves tracer to
    /// a log archive.
    /// </summary>
    public sealed class LogArchiveTracer : ILogArchiveTracer
    {
        private readonly object logFileLock = new object();
        private readonly PageableLog log;
        private ZipArchive logZipArchive;
        private Stream logFileStream;
        private StreamWriter tracerStreamWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogArchiveTracer"/> class.
        /// </summary>
        /// <param name="log">The log file this tracer traces to.</param>
        public LogArchiveTracer(PageableLog log)
        {
            this.log = log;
            this.logZipArchive = ZipFile.Open(this.log.ArchiveFileName, ZipArchiveMode.Update);
            ZipArchiveEntry logFileEntry = this.logZipArchive.Entries.FirstOrDefault(entry => entry.Name == this.log.Name) ??
                                           this.logZipArchive.CreateEntry(this.log.Name, CompressionLevel.Optimal);

            this.logFileStream = logFileEntry.Open();
            this.tracerStreamWriter = new StreamWriter(this.logFileStream);
        }

        #region Implementation of ITracer

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId => this.log.Name;

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

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.tracerStreamWriter?.Dispose();
            this.tracerStreamWriter = null;

            this.logFileStream?.Dispose();
            this.logFileStream = null;

            this.logZipArchive?.Dispose();
            this.logZipArchive = null;
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Trace <paramref name="message"/> with the specified level.
        /// </summary>
        /// <param name="level">The trace level.</param>
        /// <param name="message">The message to trace.</param>
        private void Trace(TraceLevel level, string message)
        {
            lock (this.logFileLock)
            {
                var traceLine = new TraceLine(level, DateTime.UtcNow, message);

                // Write the trace line to the log file
                this.tracerStreamWriter.WriteLine(traceLine.Compose());
                this.tracerStreamWriter.Flush();

                // And to log object
                this.log.AddTraceLine(traceLine);
            }
        }

        #endregion
    }
}
