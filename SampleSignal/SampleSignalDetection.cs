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
        [DetectionPresentation(DetectionPresentationSection.Property, "Property Value", InfoBalloon = "Property Value tootltip(in details/summary) for {MachineName}", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
        public double Value => 40.10;

        [DetectionPredicate]
        [DetectionPresentation(DetectionPresentationSection.Property, "Property Machine name", InfoBalloon = "Property MachineName tootltip(in details)")]
        public string MachineName => "The greatest machine in the world!";

        [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis Value2", InfoBalloon = "Analysis Value2(in details)")]
        public string Value2 => "What is my value in this world?";

        [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis Value3", InfoBalloon = "Analysis Value3 tootltip(in details/summary)", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
        public string Value3 => "Every man has his price";

        [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis Value4", InfoBalloon = "Analysis Value4 tootltip(in summary)", Component = DetectionPresentationComponent.Summary)]
        public string Value4 => "At Face Value";

        [DetectionPresentation(DetectionPresentationSection.Chart, "Chart 1", InfoBalloon = "Chart 1 tootltip(in details)")]
        public string Query1 => "Query";//todo add query

    }
}
