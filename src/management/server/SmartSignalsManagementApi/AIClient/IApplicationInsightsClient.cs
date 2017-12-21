namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface responsible for querying Application Insights via Rest API
    /// </summary>
    public interface IApplicationInsightsClient
    {
        /// <summary>
        /// Gets all the custom events from Application Insights for the configured application by the given filtering.
        /// </summary>
        /// <param name="startTime">(optional) filtering by start time.</param>
        /// <param name="endTime">(optional) filtering by end time.</param>
        /// <param name="cancellationToken">(optional) The cancellation token.</param>
        /// <returns>The Application Insights events.</returns>
        Task<IEnumerable<ApplicationInsightsEvent>> GetCustomEventsAsync(
                                                    DateTime? startTime = null,
                                                    DateTime? endTime = null,
                                                    CancellationToken cancellationToken = default(CancellationToken));
    }
}
