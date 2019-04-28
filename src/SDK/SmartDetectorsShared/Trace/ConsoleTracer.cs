//-----------------------------------------------------------------------
// <copyright file="ConsoleTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Trace
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implementation of the <see cref="ITracer"/> interface that traces to the console.
    /// </summary>
    public class ConsoleTracer : ITracer
    {
        private readonly object consoleLocker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        public ConsoleTracer(string sessionId)
        {
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
            this.TraceToConsole(message, ConsoleColor.White);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceError(string message)
        {
            this.TraceToConsole(message, ConsoleColor.Red);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceVerbose(string message)
        {
            this.TraceToConsole(message, ConsoleColor.Gray);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceWarning(string message)
        {
            this.TraceToConsole(message, ConsoleColor.Yellow);
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
            this.TraceToConsole($"METRIC: {name}={value}", ConsoleColor.Gray);
        }

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public void ReportException(Exception exception)
        {
            this.TraceToConsole($"EXCEPTION: {exception}", ConsoleColor.Red);
        }

        /// <summary>
        /// Send information about a dependency handled by the application.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken by the application to handle the dependency.</param>
        /// <param name="success">Was the dependency call successful</param>
        /// <param name="metrics">Named double values to add additional metric info to the dependency</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            this.TraceToConsole($"--> name={dependencyName}, command={commandName}, duration={duration}, success={success}", success ? ConsoleColor.Gray : ConsoleColor.Red);
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Dictionary of application-defined metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.TraceToConsole($"EVENT: name={eventName}", ConsoleColor.Gray);
        }

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
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

        #region Private helper methods

        /// <summary>
        /// Trace the message to the console
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="foregroundColor">The foreground color to use</param>
        private void TraceToConsole(string message, ConsoleColor foregroundColor)
        {
            lock (this.consoleLocker)
            {
                ConsoleColor originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(message);
                Console.ForegroundColor = originalForeground;
            }
        }

        #endregion
    }
}