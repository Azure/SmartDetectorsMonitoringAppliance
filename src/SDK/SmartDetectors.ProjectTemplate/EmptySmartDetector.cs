namespace $safeprojectname$
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// A sample implementation of a <see cref="ISmartDetector"/>.	
    /// This sample implementation always returns a single alert with a default title.
    /// </summary>
    public class $detectorName$ : ISmartDetector
    {
        /// <summary>
        /// Initiates an asynchronous operation for analyzing the Smart Detector on the specified resources.
        /// </summary>
        /// <param name="analysisRequest">The analysis request data.</param>
        /// <param name="tracer">
        /// A tracer used for emitting telemetry from the Smart Detector's execution. This telemetry will be used for troubleshooting and
        /// monitoring the Smart Detector's executions.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation, returning the Alerts detected for the target resources. 
        /// </returns>
        public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            var alerts = new List<Alert>
            {
                { new $alertName$("title", analysisRequest.RequestParameters.First()) }
            };

            return Task.FromResult(alerts);
        }
    }
}