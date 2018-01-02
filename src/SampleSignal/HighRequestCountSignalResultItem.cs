//-----------------------------------------------------------------------
// <copyright file="HighRequestCountSignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{    
    /// <summary>
    /// A class representing the result of a sample signal 
    /// </summary>
    class HighRequestCountSignalResultItem : SmartSignalResultItem
    {
        // The max request count - fetched from application insights
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Maximum Request Count for the application", InfoBalloon = "Maximum requests for application '{AppName}'", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public long RequestCount { get; set; }

        // The name of the application with the maximum requests in the last day - fetched from application insights
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "App Name", InfoBalloon = "App Name", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public string AppName {get; set; }

        // The highest Prcoessor Time percentage in the last hour - fetched from log analytics
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Highest Processor Time Percentage", InfoBalloon = "The highest Processor Time percentage", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public double HighestProcessorTimePercent { get; set; }

        // The time of the highest Processor Time percentage in the last hour - fetched from log analytics
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Highest CPU Percentage Time", InfoBalloon = "When was the highest CPU percentage?", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public DateTime TimeOfHighestCpu { get; set; }

        // Describe the problem
        [ResultItemPresentation(ResultItemPresentationSection.Analysis, "What is the problem?", InfoBalloon = "The problem analysis", Component = ResultItemPresentationComponent.Details)]
        public string Problem => "There was an unusually high request count for the application";

        // Time chart to appear in the details component - fetched from Log Analytics by default
        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Time Chart", InfoBalloon = "Time Chart showing requests per app in the last day", Component = ResultItemPresentationComponent.Details)]
        public string DetailQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render timechart ";

        // Bar chart to appear in the summary component - fetched from Log Analytics by default
        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Bar Chart", InfoBalloon = "Bar Chart showing requests per app  in the last day", Component = ResultItemPresentationComponent.Summary)]
        public string SummaryQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render barchart";

        // All the data that is displayed in the charts - fetched from Log Analytics by default
        [ResultItemPresentation(ResultItemPresentationSection.AdditionalQuery, "More Information", InfoBalloon = "More Information...", Component = ResultItemPresentationComponent.Details)]
        public string AdditionalQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated";
        public HighRequestCountSignalResultItem(string title, string appName, long requestCount, double highestProcessorTimePercent, DateTime timeOfHighestCpu, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
            this.AppName = appName;
            this.RequestCount = requestCount;
            this.HighestProcessorTimePercent = highestProcessorTimePercent;
            this.TimeOfHighestCpu = timeOfHighestCpu;
        }
    }
}
