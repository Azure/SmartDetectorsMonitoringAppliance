namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class SampleSignalResultItem : SmartSignalResultItem
    {
        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Property Value", InfoBalloon = "Property Value tootltip(in details/summary)", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
        public double Value => 40.10;

        [ResultItemPredicate]
        [ResultItemPresentation(ResultItemPresentationSection.Property, "Property Machine name", InfoBalloon = "Property MachineName tootltip(in details)")]
        public string MachineName => "The greatest machine in the world!";

        [ResultItemPresentation(ResultItemPresentationSection.Analysis, "Analysis Value", InfoBalloon = "Analysis Value(in details)", Component = ResultItemPresentationComponent.Details)]
        public string Value2 => "What is my value in this world?";

        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Chart 1", InfoBalloon = "Chart 1 tootltip(in details)", Component = ResultItemPresentationComponent.Details)]
        public string Query1 => "requests | where timestamp > ago(1d) | summarize count() by appName, timestamp | render timechart ";

        [ResultItemPresentation(ResultItemPresentationSection.Chart, "Chart 2", InfoBalloon = "Chart 2 tootltip(in summary)", Component = ResultItemPresentationComponent.Summary)]
        public string Query2 => "requests | where timestamp > ago(1d) | summarize count() by appName, timestamp | render barchart ";

        [ResultItemPresentation(ResultItemPresentationSection.AdditionalQuery, "AdditionalQuery 1", InfoBalloon = "AdditionalQuery 1 tootltip(in details)", Component = ResultItemPresentationComponent.Details)]
        public string Query3 => "Additional Query section....";


        public SampleSignalResultItem(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
        }
    }
}
