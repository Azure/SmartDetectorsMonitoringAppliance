namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class SampleSignalDetection : SmartSignalDetection
    {
        public override string Title => "Sample Signal title";

        [DetectionPredicate]
        [DetectionPresentation(DetectionPresentationSection.Property, "Property Value", InfoBalloon = "Property Value tootltip(in details/summary)", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
        public double Value => 40.10;

        [DetectionPredicate]
        [DetectionPresentation(DetectionPresentationSection.Property, "Property Machine name", InfoBalloon = "Property MachineName tootltip(in details)")]
        public string MachineName => "The greatest machine in the world!";

        [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis Value", InfoBalloon = "Analysis Value(in details)", Component = DetectionPresentationComponent.Details)]
        public string Value2 => "What is my value in this world?";

        [DetectionPresentation(DetectionPresentationSection.Chart, "Chart 1", InfoBalloon = "Chart 1 tootltip(in details)", Component = DetectionPresentationComponent.Details)]
        public string Query1 => "Query";//todo add query

        [DetectionPresentation(DetectionPresentationSection.Chart, "Chart 2", InfoBalloon = "Chart 2 tootltip(in summary)", Component = DetectionPresentationComponent.Summary)]
        public string Query2 => "Query";//todo add query

        [DetectionPresentation(DetectionPresentationSection.AdditionalQuery, "AdditionalQuery 1", InfoBalloon = "AdditionalQuery 1 tootltip(in details)", Component = DetectionPresentationComponent.Details)]
        public string Query3 => "Query";//todo add query


    }
}
