namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared.Trace
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interfaces for a scoped trace activity
    /// </summary>
    public interface ITraceActivity : IDisposable
    {
        /// <summary>
        /// Sets a custom property to be added to all telemetry sent in the scope of the activity.
        /// </summary>
        /// <param name="name">The custom property name.</param>
        /// <param name="value">The custom property value.</param>
        void SetCustomProperty(string name, string value);

        /// <summary>
        /// Sets custom properties to be added to all telemetry sent in the scope of the activity.
        /// </summary>
        /// <param name="properties">The custom properties.</param>
        void SetCustomProperties(IDictionary<string, string> properties);
    }
}