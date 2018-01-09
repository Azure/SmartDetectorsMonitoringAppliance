//-----------------------------------------------------------------------
// <copyright file="HighRequestCountSignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System;

    /// <summary>
    /// A class representing the result of a sample signal 
    /// </summary>
    public class HighRequestCountSignalResultItem : SmartSignalResultItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighRequestCountSignalResultItem" /> class with the values 
        /// that were fetched from the application insights and the log analytics
        /// </summary>
        /// <param name="title">The title of the signal</param>
        /// <param name="appName">The name of the application with the highest number of requests (from Application Insights)</param>
        /// <param name="requestCount">The highest number of requests (from Application Insights)</param>
        /// <param name="highestProcessorTimePercent">The highest processor time percent (from Log Analytics)</param>
        /// <param name="timeOfHighestProcessorTimePercent">The time of the highest processor time percent (from Log Analytics)</param>
        /// <param name="resourceIdentifier">The resource identifier</param>
        public HighRequestCountSignalResultItem(string title, string appName, long requestCount, double highestProcessorTimePercent, DateTime timeOfHighestProcessorTimePercent, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
            this.AppName = appName;
            this.RequestCount = requestCount;
            this.HighestProcessorTimePercent = highestProcessorTimePercent;
            this.TimeOfHighestProcessorTimePercent = timeOfHighestProcessorTimePercent;
        }

        /// <summary>
        /// Gets or sets the max request count - fetched from application insights
        /// </summary>
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Maximum Request Count for the application", InfoBalloon = "Maximum requests for application '{AppName}'", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public long RequestCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the application with the maximum requests in the last day - fetched from application insights
        /// </summary>
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "App Name", InfoBalloon = "App Name",
            Component = ResultItemPresentationComponent.Details )]
        public string AppName { get; set; }

        /// <summary>
        /// Gets or sets the highest Processor Time percentage in the last hour - fetched from log analytics
        /// </summary>
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Highest Processor Time Percentage", InfoBalloon = "The highest Processor Time percentage", Component = ResultItemPresentationComponent.Details)]
        public double HighestProcessorTimePercent { get; set; }

        /// <summary>
        /// Gets or sets the time of the highest Processor Time percentage in the last hour - fetched from log analytics
        /// </summary>
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Highest CPU Percentage Time", InfoBalloon = "When was the highest CPU percentage?", Component = ResultItemPresentationComponent.Details)]
        public DateTime TimeOfHighestProcessorTimePercent { get; set; }

        /// <summary>
        /// Describe the problem
        /// </summary>
        [ResultItemPresentation(ResultItemPresentationSection.Analysis, "What is the problem?", InfoBalloon = "The problem analysis", Component = ResultItemPresentationComponent.Details)]
        public string Problem => "There was an unusually high request count for the application";

        /// <summary>
        /// Time chart to appear in the details component - fetched from Log Analytics by default
        /// </summary>
        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Time Chart", InfoBalloon = "Time Chart showing requests per app in the last day", Component = ResultItemPresentationComponent.Details)]
        public string DetailQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render timechart ";

        /// <summary>
        /// Bar chart to appear in the summary component - fetched from Log Analytics by default
        /// </summary>
        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Bar Chart", InfoBalloon = "Bar Chart showing requests per app  in the last day", Component = ResultItemPresentationComponent.Summary)]
        public string SummaryQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated   | render barchart";

        /// <summary>
        /// All the data that is displayed in the charts - fetched from Log Analytics by default
        /// </summary>
        [ResultItemPresentation(ResultItemPresentationSection.AdditionalQuery, "More Information", InfoBalloon = "More Information...", Component = ResultItemPresentationComponent.Details)]
        public string AdditionalQuery => "Perf | where TimeGenerated  >= ago(1h) | where CounterName == '% Processor Time'| project CounterValue,TimeGenerated";
    }
}
