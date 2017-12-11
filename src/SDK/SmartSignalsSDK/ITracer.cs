namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface providing tracing capabilities. Smart Signals can use this to emit telemetry for troubleshooting and monitoring
    /// of the signal's executions.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Trace <paramref name="message"/> as Information message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        void TraceInformation(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        void TraceError(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        void TraceVerbose(string message, IDictionary<string, string> properties = null);

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        void TraceWarning(string message, IDictionary<string, string> properties = null);

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
        void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null);

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        void ReportException(Exception exception);

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
        void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null);

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Named double values that define additional event metrics</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        void Flush();
    }
}
