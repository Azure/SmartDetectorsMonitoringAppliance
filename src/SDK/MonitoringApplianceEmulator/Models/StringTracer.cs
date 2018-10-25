//-----------------------------------------------------------------------
// <copyright file="StringTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Implementation of <see cref="IObservableTracer"/> that saves all traces in a string.
    /// </summary>
    public class StringTracer : ObservableObject, IObservableTracer
    {
        private StringBuilder traceStringBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTracer"/> class.
        /// </summary>
        /// <param name="sessionId">Session id used for tracing</param>
        public StringTracer(string sessionId)
        {
            this.SessionId = sessionId;
            this.Traces = new StringBuilder();
        }

        /// <summary>
        /// Gets all traces in a single string.
        /// </summary>
        public StringBuilder Traces
        {
            get
            {
                return this.traceStringBuilder;
            }

            private set
            {
                this.traceStringBuilder = value;
                this.OnPropertyChanged();
            }
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
            this.Trace($"Information: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Error message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceError(string message)
        {
            this.Trace($"Error: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Verbose message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceVerbose(string message)
        {
            this.Trace($"Verbose: {message}");
        }

        /// <summary>
        /// Trace <paramref name="message"/> as Warning message.
        /// </summary>
        /// <param name="message">The message to trace</param>
        public void TraceWarning(string message)
        {
            this.Trace($"Warning: {message}");
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

        /// <summary>
        /// Clears all traces
        /// </summary>
        public void Clear()
        {
            this.Traces = new StringBuilder();
        }

        /// <summary>
        /// Appends a trace line to the string.
        /// </summary>
        /// <param name="message">The message to trace</param>
        private void Trace(string message)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            this.Traces = this.Traces.AppendLine($"[{timestamp}] {message}");
        }
    }
}
