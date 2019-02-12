//-----------------------------------------------------------------------
// <copyright file="WebJobTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Trace
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.WebJobs.Host;

    /// <summary>
    /// Implementation of the <see cref="IExtendedTracer"/> interface that traces to a (WebJob's) <see cref="TraceWriter"/> logger.
    /// </summary>
    public class WebJobTracer : IExtendedTracer
    {
        private readonly TraceWriter logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebJobTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="logger">The logger to send traces to</param>
        public WebJobTracer(string sessionId, TraceWriter logger)
        {
            this.logger = Diagnostics.EnsureArgumentNotNull(() => logger);
            this.SessionId = sessionId;
        }

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Trace <paramref name="message"/> as Information message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceInformation(string message)
        {
            this.logger.Info(message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceError(string message)
        {
            this.logger.Error(message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceVerbose(string message)
        {
            this.logger.Verbose(message);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceWarning(string message)
        {
            this.logger.Warning(message);
        }

        /// <summary>
        /// Reports a metric.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="value">The metric value</param>
        /// <param name="properties">Named string values you can use to classify and filter metrics</param>
        /// <param name="count">The aggregated metric count</param>
        /// <param name="max">The aggregated metric max value</param>
        /// <param name="min">The aggregated metric min name</param>
        /// <param name="timestamp">The timestamp of the aggregated metric</param>
        public void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            this.logger.Info($"Metric: name-{name}, value-{value}");
        }

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public void ReportException(Exception exception)
        {
            this.logger.Info($"Exception: {exception}");
        }

        /// <summary>
        /// Send information about a dependency call.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken to handle the dependency.</param>
        /// <param name="success">Was the dependency call successful</param>
        /// <param name="metrics">Named double values that define additional dependency metrics</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            this.logger.Info($"Dependency: name={dependencyName}, command={commandName}, duration={duration}, success={success}");
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Dictionary of application-defined metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.logger.Info($"Event: name={eventName}");
        }

        /// <summary>
        /// Flushes the telemetry
        /// </summary>
        public void Flush()
        {
            this.logger.Flush();
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
    }
}