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
        /// Reports a custom named metric.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="value">The metric value</param>
        /// <param name="properties">Named string values used to classify the metric</param>
        /// <param name="count">The aggregated metric count</param>
        /// <param name="max">The aggregated metric max value</param>
        /// <param name="min">The aggregated metric min value</param>
        /// <param name="timestamp">The aggregated metric custom timestamp</param>
        public void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            this.Trace(TraceLevel.Info, $"METRIC: {name}={value}");
        }

        /// <summary>
        /// Reports a runtime exception.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public void ReportException(Exception exception)
        {
            this.Trace(TraceLevel.Info, $"EXCEPTION: {exception}");
        }

        /// <summary>
        /// Tracks information about a dependency call.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken to handle the dependency.</param>
        /// <param name="success">A boolean value indicating whether the dependency call was successful</param>
        /// <param name="metrics">Named double values that define additional dependency metrics</param>
        /// <param name="properties">Named string values used to classify the dependency</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            this.Trace(TraceLevel.Info, $"DEPENDENCY: {dependencyName} {commandName}, startTime={startTime}, duration={duration}, succeeded={success}");
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values used to classify the event</param>
        /// <param name="metrics">Named double values that define additional event metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.Trace(TraceLevel.Info, $"EVENT: {eventName}");
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

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
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
