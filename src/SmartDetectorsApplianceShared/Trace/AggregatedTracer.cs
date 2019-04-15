//-----------------------------------------------------------------------
// <copyright file="AggregatedTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Trace
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// Implementation of the <see cref="ITracer"/> interface that traces to other <see cref="ITracer"/> objects.
    /// </summary>
    public class AggregatedTracer : ITracer
    {
        private readonly ConcurrentDictionary<ITracer, Type> tracers;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatedTracer"/> class.
        /// </summary>
        /// <param name="tracers">List of tracers to trace to</param>
        public AggregatedTracer(List<ITracer> tracers)
        {
            Diagnostics.EnsureArgumentNotNull(() => tracers);
            this.tracers = new ConcurrentDictionary<ITracer, Type>(tracers.Where(t => t != null).ToDictionary(t => t, t => t.GetType()));

            Diagnostics.EnsureArgument(this.tracers.Count > 0, () => tracers, "Must get at least one non-null tracer");
            Diagnostics.EnsureArgument(this.tracers.Keys.Select(t => t.SessionId).Distinct().Count() == 1, () => tracers, "All tracers must have the same session ID");
            this.SessionId = this.tracers.First().Key.SessionId;
        }

        #region Implementation of ITracer

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
            this.SafeCallTracers(t => t.TraceInformation(message));
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceError(string message)
        {
            this.SafeCallTracers(t => t.TraceError(message));
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceVerbose(string message)
        {
            this.SafeCallTracers(t => t.TraceVerbose(message));
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceWarning(string message)
        {
            this.SafeCallTracers(t => t.TraceWarning(message));
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
        /// <param name="timestamp">The aggregated metric timestamp</param>
        public void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            this.SafeCallTracers(t => t.ReportMetric(name, value, properties, count, max, min, timestamp));
        }

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public void ReportException(Exception exception)
        {
            this.SafeCallTracers(t => t.ReportException(exception));
        }

        /// <summary>
        /// Send information about a dependency handled by the application.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken by the application to handle the dependency.</param>
        /// <param name="success">Was the dependency call successful</param>
        /// <param name="metrics">Named double values that add additional metric info to the dependency</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        public void TrackDependency(
            string dependencyName,
            string commandName,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool success,
            IDictionary<string, double> metrics = null,
            IDictionary<string, string> properties = null)
        {
            this.SafeCallTracers(t => t.TrackDependency(dependencyName, commandName, startTime, duration, success, metrics, properties));
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Dictionary of application-defined metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.SafeCallTracers(t => t.TrackEvent(eventName, properties, metrics));
        }

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
            this.SafeCallTracers(t => t.Flush());
        }

        /// <summary>
        /// Adds a custom property, to be included in all traces.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void AddCustomProperty(string name, string value)
        {
            this.SafeCallTracers(t => t.AddCustomProperty(name, value));
        }

        /// <summary>
        /// Gets the custom properties that are set in this tracer instance.
        /// </summary>
        /// <returns>The custom properties, as a read-only dictionary</returns>
        public IReadOnlyDictionary<string, string> GetCustomProperties()
        {
            // Take all the custom properties from the inner tracers. If there are duplicate keys, take the first value.
            List<IReadOnlyDictionary<string, string>> allProperties = new List<IReadOnlyDictionary<string, string>>();
            this.SafeCallTracers(t => allProperties.Add(t.GetCustomProperties()));
            return allProperties
                .SelectMany(x => x)
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        #endregion

        /// <summary>
        /// Runs <paramref name="action"/> on all aggregated tracers in a safe way
        /// </summary>
        /// <param name="action">The action to run</param>
        private void SafeCallTracers(Action<ITracer> action)
        {
            foreach (ITracer tracer in this.tracers.Keys)
            {
                try
                {
                    action(tracer);
                }
                catch (Exception e)
                {
                    this.tracers.TryRemove(tracer, out Type tracerType);
                    if (this.tracers.Count == 0)
                    {
                        throw;
                    }

                    this.SafeCallTracers(t => t.TraceWarning($"Got exception while calling tracer {tracerType.Name}: {e}"));
                }
            }
        }
    }
}