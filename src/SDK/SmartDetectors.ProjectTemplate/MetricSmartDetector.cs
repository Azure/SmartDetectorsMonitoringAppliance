namespace $safeprojectname$
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;

    /// <summary>
    /// A sample implementation of a <see cref="ISmartDetector"/>.	
    /// This sample implementation creates alerts based on the QueueMessageCount metric.
    /// </summary>
    public class $detectorName$ : ISmartDetector
    {
        /// <summary>
        /// Initiates an asynchronous operation for running the Smart Detector analysis on the specified resources.
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
        public async Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            tracer.TraceInformation("Analyzing the specified resources...");

            // Get the metrics client
            IMetricClient metricsClient = await analysisRequest.AnalysisServicesFactory.CreateMetricClientAsync(analysisRequest.TargetResources.First(), cancellationToken);

            // Get the resource metrics. In the example below, the requested metric values are the total number of messages
            // in the storage queue over the last day, in hourly interval
            var parameters = new QueryParameters()
            {
                StartTime = DateTime.UtcNow.Date.AddDays(-1),
                EndTime = DateTime.UtcNow.Date,
                Aggregations = new List<Aggregation> { Aggregation.Total },
                MetricName = "QueueMessageCount",
                Interval = TimeSpan.FromMinutes(60)
            };

            IEnumerable<MetricQueryResult> metrics = (await metricsClient.GetResourceMetricsAsync(ServiceType.AzureStorageQueue, parameters, default(CancellationToken)));

            // Process the resource metric values and create alerts
            List<Alert> alerts = new List<Alert>();
            if (metrics.Count() > 0)
            {
                alerts.Add(new $alertName$("title", analysisRequest.TargetResources.First(), ExtendedDateTime.UtcNow));
            }

            tracer.TraceInformation($"Created {alerts.Count()} alerts");
            return alerts;
        }
    }   
}