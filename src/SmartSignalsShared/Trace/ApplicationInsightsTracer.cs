namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility;
    using Exceptions;
    using Shared;

    /// <summary>
    /// Implementation of the <see cref="ITracer"/> interface that traces to AppInsights.
    /// </summary>
    public class ApplicationInsightsTracer : ITracer
    {
        private const string TraceOrderKey = "TraceOrder";
        private static long _traceOrder = 0;

        private const int MaxExceptionLength = 20000;

        private readonly TelemetryClient _telemetryClient;
        private readonly IDictionary<string, string> _customProperties;
        private readonly bool _sendVerboseTracesToAi;

        /// <summary>
        /// Initialized a new instance of the <see cref="ApplicationInsightsTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="telemetryConfiguration">Telemetry configuration to be used by telemetry client</param>
        /// <param name="sendVerboseTracesToAi">Whether to trace verbose data (null to read this value from the config)</param>
        /// <param name="buildVersion">The build version to include in the trace properties (null to read this value from the config)</param>
        public ApplicationInsightsTracer(string sessionId, TelemetryConfiguration telemetryConfiguration, bool? sendVerboseTracesToAi = null, string buildVersion = null)
        {
            _sendVerboseTracesToAi = sendVerboseTracesToAi ?? bool.Parse(ConfigurationReader.ReadConfig("SendVerboseTracesToAI", required: true));
            _telemetryClient = this.CreateTelemetryClient(sessionId, telemetryConfiguration);
            _customProperties = new Dictionary<string, string>
            {
                ["WebAppSiteName"] = AzureFunctionEnvironment.WebAppSiteName ?? string.Empty,
                ["BuildVersion"] = buildVersion ?? ConfigurationReader.ReadConfig("BuildVersion", required: true),
                ["MachineName"] = Environment.MachineName,
                ["WebsiteInstanceId"] = AzureFunctionEnvironment.WebsiteInstanceId ?? string.Empty
            };

            this.SessionId = sessionId;
            this.OperationHandler = new ApplicationInsightsRequestOperationHandler(_telemetryClient, _customProperties);
        }

        /// <summary>
        /// Gets the tracer's operation handler
        /// </summary>
        public ITelemetryOperationHandler OperationHandler { get; protected set; }

        /// <summary>
        /// Gets the tracer's session ID
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Trace <paramref name="message"/> as <see cref="SeverityLevel.Information"/> message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public virtual void TraceInformation(string message, IDictionary<string, string> properties = null)
        {
            this.Trace(message, SeverityLevel.Information, properties);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as <see cref="SeverityLevel.Error"/> message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public virtual void TraceError(string message, IDictionary<string, string> properties = null)
        {
            this.Trace(message, SeverityLevel.Error, properties);
        }

        /// <summary>
        /// Trace <paramref name="message"/> as <see cref="SeverityLevel.Verbose"/> message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public virtual void TraceVerbose(string message, IDictionary<string, string> properties = null)
        {
            if (_sendVerboseTracesToAi)
            {
                this.Trace(message, SeverityLevel.Verbose, properties);
            }
        }

        /// <summary>
        /// Trace <paramref name="message"/> as <see cref="SeverityLevel.Warning"/> message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        public virtual void TraceWarning(string message, IDictionary<string, string> properties = null)
        {
            this.Trace(message, SeverityLevel.Warning, properties);
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
        public virtual void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null)
        {
            var metricTelemetry = new MetricTelemetry(name, value)
            {
                Count = count,
                Max = max,
                Min = min
            };

            // set custom timestamp if exist
            if (timestamp.HasValue)
            {
                metricTelemetry.Timestamp = timestamp.Value;
            }

            this.SetTelemetryProperties(metricTelemetry, properties);
            this.SetTelemetryContext(metricTelemetry);
            _telemetryClient.TrackMetric(metricTelemetry);
        }

        /// <summary>
        /// Reports a runtime exception.
        /// It uses exception and trace entities with same operation id.
        /// </summary>
        /// <param name="exception">The exception to report</param>
        public virtual void ReportException(Exception exception)
        {
            // Trace exception
            this.TrackTrace(exception.ToString(), SeverityLevel.Error, null);

            // Track exception
            ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(this.HandleTooLongException(exception));
            this.SetTelemetryProperties(exceptionTelemetry);
            this.SetTelemetryContext(exceptionTelemetry);

            _telemetryClient.TrackException(exceptionTelemetry);
        }

        /// <summary>
        /// Checks if the exception is too long, and cannot be traced.
        /// If it is too long, replaces it with an <see cref="ExceptionTooLongException"/>.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>Either the original exception, or an instance of <see cref="ExceptionTooLongException"/> if the original exception was too long.</returns>
        private Exception HandleTooLongException(Exception exception)
        {
            if (exception.ToString().Length <= MaxExceptionLength)
            {
                return exception;
            }

            // This id can be used to track the exception in the trace
            string referenceId = Guid.NewGuid().ToString();
            this.TrackTrace($"Exception was too long - reporting an ExceptionTooLongException with referenceId = {referenceId}", SeverityLevel.Error, null);
            return new ExceptionTooLongException(exception, referenceId);
        }

        /// <summary>
        /// Send information about a dependency handled by the application.
        /// </summary>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="startTime">The dependency call start time</param>
        /// <param name="duration">The time taken by the application to handle the dependency.</param>
        /// <param name="success">Was the dependency call successful</param>
        /// <param name="metrics">Named double values to add additional metric info about the dependency</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        public virtual void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success, IDictionary<string, double> metrics = null, IDictionary<string, string> properties = null)
        {
            var dependencyTelemetry = new DependencyTelemetry("Other", dependencyName, dependencyName, commandName, startTime, duration, success ? "Success" : "Failure", success);
            this.SetTelemetryProperties(dependencyTelemetry, properties);
            this.SetTelemetryContext(dependencyTelemetry);

            if (metrics != null)
            {
                foreach (KeyValuePair<string, double> metric in metrics)
                {
                    dependencyTelemetry.Metrics.Add(metric.Key, metric.Value);
                }
            }

            _telemetryClient.TrackDependency(dependencyTelemetry);
        }

        /// <summary>
        /// Send information about an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <param name="metrics">Dictionary of application-defined metrics</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var eventTelemetry = new EventTelemetry(eventName);
            this.SetTelemetryProperties(eventTelemetry, properties);
            this.SetTelemetryContext(eventTelemetry);

            if (metrics != null)
            {
                foreach (KeyValuePair<string, double> metric in metrics)
                {
                    eventTelemetry.Metrics.Add(metric.Key, metric.Value);
                }
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Creates a new activity scope, and returns an <see cref="IDisposable"/> activity.
        /// </summary>
        /// <returns>A disposable activity object to control the end of the activity scope</returns>
        public ITraceActivity CreateNewActivityScope()
        {
            // Create a new activity ID
            Guid newActivityId = Guid.NewGuid();

            // Get the current activity ID
            string oldActivityId = TraceActivity.GetCurrentActivityId();

            // Create the new activity
            var newActivity = new TraceActivity(newActivityId, this);
            this.Trace($"Activity was transferred from {oldActivityId} to {newActivityId}", SeverityLevel.Information, null);

            return newActivity;
        }

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
            _telemetryClient.Flush();
        }

        #region Private helper methods

        /// <summary>
        /// Creates the telemetry client with the correct endpoint and sets the session id.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="telemetryConfiguration">Telemetry configuration to be used by telemetry client</param>
        private TelemetryClient CreateTelemetryClient(string sessionId, TelemetryConfiguration telemetryConfiguration)
        {
            var telemetryClient = new TelemetryClient(telemetryConfiguration);

            telemetryClient.Context.Session.Id = sessionId;

            return telemetryClient;
        }

        /// <summary>
        /// Traces the specified message to the configured targets
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="severityLevel">The message's severity level</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        private void Trace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            this.TrackTrace(message, severityLevel, properties);
        }

        /// <summary>
        /// Traces the specified message to the telemetry client only
        /// </summary>
        /// <param name="message">The message to trace</param>
        /// <param name="severityLevel">The message's severity level</param>
        /// <param name="properties">Named string values you can use to classify and filter traces</param>
        private void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            var traceTelemetry = new TraceTelemetry(message, severityLevel);

            // Set telemetry properties and context
            this.SetTelemetryProperties(traceTelemetry, properties);
            this.SetTelemetryContext(traceTelemetry);

            // Add trace order - running number, to enable sorting trace messages that have the same timestamp
            long traceOrder = Interlocked.Increment(ref _traceOrder);
            traceTelemetry.Properties[TraceOrderKey] = traceOrder.ToString();

            _telemetryClient.TrackTrace(traceTelemetry);
        }

        /// <summary>
        /// Populates the context in <paramref name="telemetry"/> based on the current operation <see cref="ApplicationInsightsRequestOperationHandler"/>,
        /// and common framework properties
        /// </summary>
        /// <param name="telemetry">The telemetry to set properties in</param>
        private void SetTelemetryContext(ITelemetry telemetry)
        {
            var operationHandler = (ApplicationInsightsRequestOperationHandler)this.OperationHandler;

            operationHandler?.SetTelemetryOperationContext(telemetry);
        }

        /// <summary>
        /// Sets the custom properties in <paramref name="telemetry"/> based on the user-supplied properties,
        /// and common framework properties
        /// </summary>
        /// <param name="telemetry">The telemetry to set properties in</param>
        /// <param name="properties">The user-supplied properties</param>
        private void SetTelemetryProperties(ISupportProperties telemetry, IDictionary<string, string> properties = null)
        {
            // Add the framework's custom properties
            foreach (KeyValuePair<string, string> customProperty in _customProperties)
            {
                telemetry.Properties[customProperty.Key] = customProperty.Value;
            }

            // Add the activity ID and properties
            telemetry.Properties["ActivityId"] = TraceActivity.GetCurrentActivityId();
            TraceActivity.AddCurrentActivityProperties(telemetry.Properties);

            // And finally, add the user-supplied properties
            if (properties != null)
            {
                foreach (KeyValuePair<string, string> customProperty in properties)
                {
                    telemetry.Properties[customProperty.Key] = customProperty.Value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Inner class representing a trace activity that can be pushed on and popped from <see cref="CallContext"/>.
        /// </summary>
        private sealed class TraceActivity : ITraceActivity
        {
            private const string TraceActivityCallContextName = "ApplicationInsights.DeepInsights.Shared.ApplicationInsightsTracer.TraceActivity";
            private const string RootActivityId = "root activity";

            /// <summary>
            /// Gets the activity ID
            /// </summary>
            private Guid _activityId;

            /// <summary>
            /// The previous activity on the stack
            /// </summary>
            private readonly TraceActivity _previous;

            /// <summary>
            /// The tracer to use when disposing
            /// </summary>
            private readonly ITracer _tracer;

            /// <summary>
            /// Optional custom properties to add the traces in this activity.
            /// </summary>
            private readonly Dictionary<string, string> _properties;

            /// <summary>
            /// Initializes a new instance of the <see cref="TraceActivity"/> class.
            /// Pushes the new activity on the call context stack
            /// </summary>
            /// <param name="activityId">The activity ID</param>
            /// <param name="tracer">The tracer to use</param>
            public TraceActivity(Guid activityId, ITracer tracer)
            {
                // Setup everything
                _activityId = activityId;
                _tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
                _previous = CallContext.LogicalGetData(TraceActivityCallContextName) as TraceActivity;
                _properties = new Dictionary<string, string>();

                // And push to the call stack
                CallContext.LogicalSetData(TraceActivityCallContextName, this);
            }

            /// <summary>
            /// Retrieves the current trace activity ID. 
            /// Can be <code>null</code> if it was never set (e.g. no call to <see cref="CreateNewActivityScope"/> was made)
            /// </summary>
            /// <returns>The current activity ID or <code>null</code> if none</returns>
            public static string GetCurrentActivityId()
            {
                TraceActivity currentActivity = CallContext.LogicalGetData(TraceActivityCallContextName) as TraceActivity;
                return SafeGetActivityId(currentActivity);
            }

            /// <summary>
            /// Adds the current trace activity properties to <paramref name="properties"/>. 
            /// </summary>
            /// <param name="properties">The dictionary of properties to add the activity properties to.</param>
            public static void AddCurrentActivityProperties(IDictionary<string, string> properties)
            {
                TraceActivity currentActivity = CallContext.LogicalGetData(TraceActivityCallContextName) as TraceActivity;
                Dictionary<string, string> activityProperties = currentActivity?._properties;
                if (activityProperties != null)
                {
                    foreach (KeyValuePair<string, string> property in activityProperties)
                    {
                        properties[property.Key] = property.Value;
                    }
                }
            }

            #region Implementation of ITraceActivity

            /// <summary>
            /// Sets a custom property to be added to all telemetry sent in the scope of the activity.
            /// </summary>
            /// <param name="name">The custom property name.</param>
            /// <param name="value">The custom property value.</param>
            public void SetCustomProperty(string name, string value)
            {
                _properties[name] = value;
            }

            /// <summary>
            /// Sets custom properties to be added to all telemetry sent in the scope of the activity.
            /// </summary>
            /// <param name="properties">The custom properties.</param>
            public void SetCustomProperties(IDictionary<string, string> properties)
            {
                foreach (KeyValuePair<string, string> property in properties)
                {
                    _properties[property.Key] = property.Value;
                }
            }

            #endregion

            #region Implementation of IDisposable

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// Pops the current activity from the call context stack
            /// </summary>
            public void Dispose()
            {
                // Get the old activity ID
                string oldActivityId = SafeGetActivityId(_previous);

                // Pop the activity from the stack
                CallContext.LogicalSetData(TraceActivityCallContextName, _previous);
                _tracer.TraceInformation($"Activity was transferred back from {_activityId} to {oldActivityId}");
            }

            #endregion

            /// <summary>
            /// Retrieves the Id of <paramref name="activity"/> in a null-safe way
            /// </summary>
            /// <param name="activity">The activity</param>
            /// <returns>The activity ID, or <see cref="RootActivityId"/></returns>
            private static string SafeGetActivityId(TraceActivity activity)
            {
                return activity?._activityId.ToString() ?? RootActivityId;
            }
        }
    }
}
