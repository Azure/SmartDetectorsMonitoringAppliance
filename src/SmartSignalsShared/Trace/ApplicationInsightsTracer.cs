//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsTracer.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;

    /// <summary>
    /// Implementation of the <see cref="ITracer"/> interface that traces to AppInsights.
    /// </summary>
    public class ApplicationInsightsTracer : ITracer
    {
        private const int MaxExceptionLength = 20000;

        private const string TraceOrderKey = "TraceOrder";
        private static long traceOrder = 0;

        private readonly TelemetryClient telemetryClient;
        private readonly IDictionary<string, string> customProperties;
        private readonly bool sendVerboseTracesToAi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="telemetryConfiguration">Telemetry configuration to be used by telemetry client</param>
        /// <param name="sendVerboseTracesToAi">Whether to trace verbose data (null to read this value from the config)</param>
        /// <param name="buildVersion">The build version to include in the trace properties (null to read this value from the config)</param>
        public ApplicationInsightsTracer(string sessionId, TelemetryConfiguration telemetryConfiguration, bool? sendVerboseTracesToAi = null, string buildVersion = null)
        {
            this.sendVerboseTracesToAi = sendVerboseTracesToAi ?? bool.Parse(ConfigurationReader.ReadConfig("SendVerboseTracesToAI", required: true));
            this.telemetryClient = this.CreateTelemetryClient(sessionId, telemetryConfiguration);
            this.customProperties = new Dictionary<string, string>
            {
                ["WebAppSiteName"] = AzureFunctionEnvironment.WebAppSiteName ?? string.Empty,
                ["BuildVersion"] = (buildVersion ?? ConfigurationReader.ReadConfig("BuildVersion", required: false)) ?? string.Empty,
                ["MachineName"] = Environment.MachineName,
                ["ProcessId"] = Process.GetCurrentProcess().Id.ToString(),
                ["WebsiteInstanceId"] = AzureFunctionEnvironment.WebsiteInstanceId ?? string.Empty
            };

            this.SessionId = sessionId;
        }

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
            if (this.sendVerboseTracesToAi)
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
            this.telemetryClient.TrackMetric(metricTelemetry);
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

            this.telemetryClient.TrackException(exceptionTelemetry);
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

            if (metrics != null)
            {
                foreach (KeyValuePair<string, double> metric in metrics)
                {
                    dependencyTelemetry.Metrics.Add(metric.Key, metric.Value);
                }
            }

            this.telemetryClient.TrackDependency(dependencyTelemetry);
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

            if (metrics != null)
            {
                foreach (KeyValuePair<string, double> metric in metrics)
                {
                    eventTelemetry.Metrics.Add(metric.Key, metric.Value);
                }
            }

            this.telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Flushes the telemetry channel
        /// </summary>
        public void Flush()
        {
            this.telemetryClient.Flush();
        }

        /// <summary>
        /// Adds a custom property, to be included in all traces.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public void AddCustomProperty(string name, string value)
        {
            this.customProperties[name] = value;
        }

        #region Private helper methods

        /// <summary>
        /// Creates the telemetry client with the correct endpoint and sets the session id.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        /// <param name="telemetryConfiguration">Telemetry configuration to be used by telemetry client</param>
        /// <returns>The newly created telemetry client.</returns>
        private TelemetryClient CreateTelemetryClient(string sessionId, TelemetryConfiguration telemetryConfiguration)
        {
            var newTelemetryClient = new TelemetryClient(telemetryConfiguration);

            newTelemetryClient.Context.Session.Id = sessionId;

            return newTelemetryClient;
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

            // Add trace order - running number, to enable sorting trace messages that have the same timestamp
            long currentTraceOrder = Interlocked.Increment(ref traceOrder);
            traceTelemetry.Properties[TraceOrderKey] = currentTraceOrder.ToString();

            this.telemetryClient.TrackTrace(traceTelemetry);
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
            foreach (KeyValuePair<string, string> customProperty in this.customProperties)
            {
                telemetry.Properties[customProperty.Key] = customProperty.Value;
            }

            // And finally, add the user-supplied properties
            if (properties != null)
            {
                foreach (KeyValuePair<string, string> customProperty in properties)
                {
                    telemetry.Properties[customProperty.Key] = customProperty.Value;
                }
            }
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

        #endregion
    }
}
