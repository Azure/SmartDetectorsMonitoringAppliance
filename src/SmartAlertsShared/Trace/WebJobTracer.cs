namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared.Trace
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using WebJobs.Host;

    /// <summary>
    /// Implementation of the <see cref="ITracer"/> interface that traces to a (WebJob's) <see cref="TraceWriter"/> logger.
    /// </summary>
    public class WebJobTracer : ITracer
    {
        private readonly TraceWriter _logger;

        /// <summary>
        /// Initialized a new instance of the <see cref="WebJobTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="logger">The logger to send traces to</param>
        public WebJobTracer(string sessionId, TraceWriter logger)
        {
            _logger = Diagnostics.EnsureArgumentNotNull(() => logger);
            this.SessionId = sessionId;
        }

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId { get; }

        public void TraceInformation(string message, IDictionary<string, string> properties = null)
        {
            _logger.Info(message);
        }

        public void TraceError(string message, IDictionary<string, string> properties = null)
        {
            _logger.Error(message);
        }

        public void TraceVerbose(string message, IDictionary<string, string> properties = null)
        {
            _logger.Verbose(message);
        }

        public void TraceWarning(string message, IDictionary<string, string> properties = null)
        {
            _logger.Warning(message);
        }

        public void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            _logger.Info($"Metric: name-{name}, value-{value}");
        }

        public void ReportException(Exception exception)
        {
            _logger.Info($"Exception: {exception}");
        }

        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            _logger.Info($"Dependency: name={dependencyName}, command={commandName}, duration={duration}, success={success}");
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Dictionary of application-defined metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            _logger.Info($"Event: name={eventName}");
        }

        /// <summary>
        /// Flushes the telemetry
        /// </summary>
        public void Flush()
        {
            _logger.Flush();
        }
    }
}