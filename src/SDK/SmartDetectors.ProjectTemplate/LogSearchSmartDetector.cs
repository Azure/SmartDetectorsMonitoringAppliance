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
    /// This sample implementation provides an example of a detector for detecting alerts based on data in $dataType$
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
            $if$ ("$dataType$" == "Log Analytics")
            // Get the Log Analytics client
            ITelemetryDataClient dataClient = await analysisRequest.AnalysisServicesFactory.CreateLogAnalyticsTelemetryDataClientAsync(new List<ResourceIdentifier>() { analysisRequest.TargetResources.First() }, cancellationToken);
            $else$
            // Get the Application Insights client
            ITelemetryDataClient dataClient = await analysisRequest.AnalysisServicesFactory.CreateApplicationInsightsTelemetryDataClientAsync(new List<ResourceIdentifier>() { analysisRequest.TargetResources.First() }, cancellationToken);
            $endif$
            // Run the query 
            IList<DataTable> dataTables = await dataClient.RunQueryAsync(@"$tableName$ | count", cancellationToken);

            // Process the query results and create alerts
            List<Alert> alerts = new List<Alert>();
            if (dataTables[0].Rows.Count > 0)
            {
                alerts.Add(new $alertName$("Title", analysisRequest.TargetResources.First(), Convert.ToInt32(dataTables[0].Rows[0]["Count"]), ExtendedDateTime.UtcNow));
            }

            tracer.TraceInformation($"Created {alerts.Count()} alerts");
            return alerts;
        }
    }
}