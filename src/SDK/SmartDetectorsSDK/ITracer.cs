//-----------------------------------------------------------------------
// <copyright file="ITracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface providing tracing capabilities. Smart Detectors can use this to emit telemetry for troubleshooting and monitoring
    /// of the Smart Detector's executions.
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
        void TraceInformation(string message);

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        void TraceError(string message);

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        void TraceVerbose(string message);

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        void TraceWarning(string message);

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
        void ReportMetric(string name, double value, IDictionary<string, string> properties = null, int? count = null, double? max = null, double? min = null, DateTime? timestamp = null);

        /// <summary>
        /// Adds a custom property, to be included in all traces.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        void AddCustomProperty(string name, string value);

        /// <summary>
        /// Gets the custom properties that are set in this tracer instance.
        /// </summary>
        /// <returns>The custom properties, as a read-only dictionary</returns>
        IReadOnlyDictionary<string, string> GetCustomProperties();
    }
}
