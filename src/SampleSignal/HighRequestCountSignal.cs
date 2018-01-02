using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Sample signal logic, tests fetching from different clients and result presentation
    /// </summary>
    public class HighRequestCountSignal : ISmartSignal

    {
        private const string MaximumProcessorTimeLogAnalyticsQuery =
            "Perf " +
            "| where TimeGenerated  >= ago(1h)" +
            "| where CounterName == '% Processor Time'" +
            "| summarize arg_max(CounterValue, TimeGenerated) ";

        private const string HighestAppApplicationInsightsQuery =
            "requests" +
            "| where timestamp > ago(1d)" +
            "| summarize countReqByAppName = count() by appName"+
            "| summarize arg_max(countReqByAppName, appName)";


        /// <summary>
        /// This method tests the signal analysis services
        /// </summary>
        public async Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            SmartSignalResult smartSignalResult = new SmartSignalResult();
            // 1. Fetch from Application Insights client - name of an application with maximum request in the last 24h
            ITelemetryDataClient applicationInsightsDataClient = await analysisRequest.AnalysisServicesFactory.CreateApplicationInsightsTelemetryDataClientAsync(
                analysisRequest.TargetResources, cancellationToken);
            IList<DataTable> aiResult = await applicationInsightsDataClient.RunQueryAsync(HighestAppApplicationInsightsQuery,cancellationToken);
            long countReqByAppName = 0;
            string appName = "N/A";
            if (aiResult.Count > 0)
            {
                countReqByAppName = Convert.ToInt64(aiResult[0].Rows[0]["countReqByAppName"]);
                appName = Convert.ToString(aiResult[0].Rows[0]["appName"]);
                tracer.TraceInformation($"App {appName} has high request count of {countReqByAppName}");
            }
            else
            {
                tracer.TraceError("Failed to perform the query in Application Insights");
            }

            // 2. Fetch from Log Analytics - time of highest CPU in the last 24h
            ITelemetryDataClient logAnalyticsDataClient = await analysisRequest.AnalysisServicesFactory.CreateLogAnalyticsTelemetryDataClientAsync(
                analysisRequest.TargetResources, cancellationToken);
            IList<DataTable> laResult = await logAnalyticsDataClient.RunQueryAsync(MaximumProcessorTimeLogAnalyticsQuery, cancellationToken);
            double highestProcessorTimePercent = 0;
            DateTime timeOfHighestProcessorTime = new DateTime();
            if (laResult.Count > 0)
            {
                highestProcessorTimePercent = Convert.ToDouble(laResult[0].Rows[0]["CounterValue"]);
                timeOfHighestProcessorTime = Convert.ToDateTime(laResult[0].Rows[0]["TimeGenerated"]);
                tracer.TraceInformation($"The highest value of % Processor Time {highestProcessorTimePercent} appeared at {timeOfHighestProcessorTime}");
            }
            else
            {
                tracer.TraceError("Failed to perform the query in Log Analytics");
            }

            analysisRequest.TargetResources.ForEach(resourceIdentifier => smartSignalResult.ResultItems.Add(
                new HighRequestCountSignalResultItem("High Processing Time Percentage(LA) and Request Count(AI)",appName,countReqByAppName, highestProcessorTimePercent, timeOfHighestProcessorTime, resourceIdentifier)));
            return smartSignalResult;
        }
    }
}
