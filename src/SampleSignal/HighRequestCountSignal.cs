//-----------------------------------------------------------------------
// <copyright file="HighRequestCountSignal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Sample signal logic, tests fetching from different clients and result presentation
    /// </summary>
    public class HighRequestCountSignal : ISmartSignal
    {
        private const string MaximumProcessorTimeLogAnalyticsQuery =
            "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time' | summarize arg_max(CounterValue, TimeGenerated) ";

        private const string HighestAppApplicationInsightsQuery =
            "requests | where timestamp > ago(1d) | summarize countReqByAppName = count() by appName | summarize arg_max(countReqByAppName, appName)";

        /// <summary>
        /// The method runs the sample analysis calls
        /// </summary>
        /// <param name="analysisRequest">The analysis request object</param>
        /// <param name="tracer">used to save trace messages</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The smart signal result containing the result items</returns>
        public async Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            SmartSignalResult smartSignalResult = new SmartSignalResult();

            // 1. Fetch from Application Insights client - name of an application with maximum request in the last 24h
            ITelemetryDataClient applicationInsightsDataClient = await analysisRequest.AnalysisServicesFactory.CreateApplicationInsightsTelemetryDataClientAsync(
                analysisRequest.TargetResources, cancellationToken);
            IList<DataTable> applicationInsightsResult = await applicationInsightsDataClient.RunQueryAsync(HighestAppApplicationInsightsQuery, cancellationToken);
            long countReqByAppName = 0;
            string appName = "N/A";
            if (applicationInsightsResult.Count > 0)
            {
                countReqByAppName = Convert.ToInt64(applicationInsightsResult[0].Rows[0]["countReqByAppName"]);
                appName = Convert.ToString(applicationInsightsResult[0].Rows[0]["appName"]);
                tracer.TraceInformation($"App {appName} has high request count of {countReqByAppName}");
            }
            else
            {
                tracer.TraceError("Failed to perform the query in Application Insights");
            }

            // 2. Fetch from Log Analytics - time of highest CPU in the last 24h
            ITelemetryDataClient logAnalyticsDataClient = await analysisRequest.AnalysisServicesFactory.CreateLogAnalyticsTelemetryDataClientAsync(
                analysisRequest.TargetResources, cancellationToken);
            IList<DataTable> logAnalyticsResult = await logAnalyticsDataClient.RunQueryAsync(MaximumProcessorTimeLogAnalyticsQuery, cancellationToken);
            double highestProcessorTimePercent = 0;
            DateTime timeOfHighestProcessorTime = new DateTime();
            if (logAnalyticsResult.Count > 0)
            {
                highestProcessorTimePercent = Convert.ToDouble(logAnalyticsResult[0].Rows[0]["CounterValue"]);
                timeOfHighestProcessorTime = Convert.ToDateTime(logAnalyticsResult[0].Rows[0]["TimeGenerated"]);
                tracer.TraceInformation($"The highest value of % Processor Time {highestProcessorTimePercent} appeared at {timeOfHighestProcessorTime}");
            }
            else
            {
                tracer.TraceError("Failed to perform the query in Log Analytics");
            }

            analysisRequest.TargetResources.ForEach(resourceIdentifier => smartSignalResult.ResultItems.Add(
                new HighRequestCountSignalResultItem("High Processing Time Percentage(LA) and Request Count(AI)", appName, countReqByAppName, highestProcessorTimePercent, timeOfHighestProcessorTime, resourceIdentifier)));
            return smartSignalResult;
        }
    }
}
