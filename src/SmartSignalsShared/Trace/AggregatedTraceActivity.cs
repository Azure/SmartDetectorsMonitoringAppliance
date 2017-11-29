namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Part of the <see cref="AggregatedTracer "/> class
    /// </summary>
    public partial class AggregatedTracer
    {
        /// <summary>
        /// Inner class representing an aggregated trace activity.
        /// </summary>
        private sealed class AggregatedTraceActivity : ITraceActivity
        {
            private List<ITraceActivity> _activities;

            /// <summary>
            /// Initializes a new instance of the <see cref="AggregatedTraceActivity"/> class.
            /// </summary>
            /// <param name="activities">The aggregated activities</param>
            public AggregatedTraceActivity(IEnumerable<ITraceActivity> activities)
            {
                Diagnostics.EnsureArgumentNotNull(() => activities);

                _activities = activities.Where(a => a != null).ToList();
            }

            #region Implementation of ITraceActivity

            /// <summary>
            /// Sets a custom property to be added to all telemetry sent in the scope of the activity.
            /// </summary>
            /// <param name="name">The custom property name.</param>
            /// <param name="value">The custom property value.</param>
            public void SetCustomProperty(string name, string value)
            {
                _activities.ForEach(activity => activity.SetCustomProperty(name, value));
            }

            /// <summary>
            /// Sets custom properties to be added to all telemetry sent in the scope of the activity.
            /// </summary>
            /// <param name="properties">The custom properties.</param>
            public void SetCustomProperties(IDictionary<string, string> properties)
            {
                _activities.ForEach(activity => activity.SetCustomProperties(properties));
            }

            #endregion

            #region Implementation of IDisposable

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// Pops the current activity from the call context stack
            /// </summary>
            public void Dispose()
            {
                if (_activities != null)
                {
                    foreach (ITraceActivity activity in _activities)
                    {
                        activity.Dispose();
                    }
                }

                _activities = null;
            }

            #endregion
        }
    }
}