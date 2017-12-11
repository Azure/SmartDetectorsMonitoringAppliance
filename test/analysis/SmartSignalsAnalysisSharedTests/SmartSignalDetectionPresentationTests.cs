namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis.DetectionPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SmartSignalDetectionPresentationTests
    {
        private const string SignalName = "signalName";

        [TestMethod]
        public void WhenProcessingDetectionThenThePresentationIsCreatedCorrectly()
        {
            DateTime dataEndTime = DateTime.Now.Date;
            string resourceId = "resourceId";
            var request = new SmartSignalRequest(new List<string>() { resourceId }, "signalId", dataEndTime.AddDays(-1), dataEndTime, new SmartSignalSettings());
            var detection = new TestDetection();
            var presentation = SmartSignalDetectionPresentation.CreateFromDetection(request, SignalName, detection);
            Assert.AreEqual(dataEndTime, presentation.AnalysisTimestamp, "Unexpected data end time");
            Assert.AreEqual(24 * 60, presentation.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SignalName, presentation.SignalName, "Unexpected signal name");
            Assert.AreEqual("Test title", presentation.Title, "Unexpected title");
            Assert.AreEqual("<the query>", presentation.Summary.Chart.Value, "Unexpected chart query");
            Assert.AreEqual(8, presentation.Properties.Count, "Unexpected number of properties");
            this.VerifyProperty(presentation.Properties, "Machine name", DetectionPresentationSection.Property, "strongOne", "The machine on which the CPU had increased");
            this.VerifyProperty(presentation.Properties, "CPU over the last 7 days", DetectionPresentationSection.Chart, "<the query>", "CPU chart for machine strongOne, showing increase of 22.4");
            this.VerifyProperty(presentation.Properties, "CPU increased", DetectionPresentationSection.Property, "22.4", "CPU increase on machine strongOne");
            this.VerifyProperty(presentation.Properties, "Another query 1", DetectionPresentationSection.AdditionalQuery, "<query1>", "Info balloon for another query 1");
            this.VerifyProperty(presentation.Properties, "Another query 2", DetectionPresentationSection.AdditionalQuery, "<query2>", "Info balloon for another query 2");
            this.VerifyProperty(presentation.Properties, "Analysis 1", DetectionPresentationSection.Analysis, "analysis1", "Info balloon for analysis 1");
            this.VerifyProperty(presentation.Properties, "Analysis 2", DetectionPresentationSection.Analysis, "analysis2", "Info balloon for analysis 2");
            this.VerifyProperty(presentation.Properties, "Analysis 3", DetectionPresentationSection.Analysis, (new DateTime(2012, 11, 12, 17, 22, 37)).ToString("u"), "Info balloon for analysis 3");
            Assert.AreEqual("no show", presentation.RawProperties["NoPresentation"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDetectionPresentationException))]
        public void WhenProcessingDetectionWithoutSummaryPropertyThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestDetectionNoSummary());
        }

        [TestMethod]
        public void WhenProcessingDetectionWithoutSummaryChartThenNoExceptionIsThrown()
        {
            this.CreatePresentation(new TestDetectionNoSummaryChart());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDetectionPresentationException))]
        public void WhenProcessingDetectionWithTwoSummaryPropertiesThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestDetectionTwoSummaryProperties());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDetectionPresentationException))]
        public void WhenProcessingDetectionWithTwoSummaryChartsThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestDetectionTwoSummaryCharts());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDetectionPresentationException))]
        public void WhenProcessingDetectionWithSummaryInWrongSectionThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestDetectionInvalidSummarySection());
        }

        [TestMethod]
        public void WhenDetectionsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var detection1 = new TestDetection();
            var detection2 = new TestDetection();
            detection2.NoPresentation += "X";

            var presentation1 = this.CreatePresentation(detection1);

            // A non predicate property is different - correlation hash should be the same
            var presentation2 = this.CreatePresentation(detection2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            detection2.OnlyPredicate += "X";
            presentation2 = this.CreatePresentation(detection2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreNotEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);
        }

        private SmartSignalDetectionPresentation CreatePresentation(SmartSignalDetection detection)
        {
            DateTime dataEndTime = DateTime.Now.Date;
            string resourceId = "resourceId";
            var request = new SmartSignalRequest(new List<string>() { resourceId }, "signalId", dataEndTime.AddDays(-1), dataEndTime, new SmartSignalSettings());
            return SmartSignalDetectionPresentation.CreateFromDetection(request, SignalName, detection);
        }

        private void VerifyProperty(List<SmartSignalDetectionPresentationProperty> properties, string name, DetectionPresentationSection displayCategory, string value, string infoBalloon)
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.IsNotNull(property, $"Property {name} not found");
            Assert.AreEqual(displayCategory, property.DisplayCategory);
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(infoBalloon, property.InfoBalloon);
        }

        private class TestDetectionNoSummary : SmartSignalDetection
        {
            public override string Title => "Test title";

            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = DetectionPresentationComponent.Details)]
            public double Value => 22.4;

            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Property, "Machine name", InfoBalloon = "The machine on which the CPU had increased")]
            public string MachineName => "strongOne";
        }

        private class TestDetectionNoSummaryProperty : TestDetectionNoSummary
        {
            [DetectionPresentation(DetectionPresentationSection.Chart, "CPU over the last 7 days", InfoBalloon = "CPU chart for machine {MachineName}, showing increase of {Value}", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
            public string CpuChartQuery => "<the query>";
        }

        private class TestDetection : TestDetectionNoSummaryProperty
        {
            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
            public new double Value => 22.4;

            [DetectionPresentation(DetectionPresentationSection.AdditionalQuery, "Another query 1", InfoBalloon = "Info balloon for another query 1")]
            public string Query1 => "<query1>";

            [DetectionPresentation(DetectionPresentationSection.AdditionalQuery, "Another query 2", InfoBalloon = "Info balloon for another query 2")]
            public string Query2 => "<query2>";

            [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis 1", InfoBalloon = "Info balloon for analysis 1")]
            public string Analysis1 => "analysis1";

            [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis 2", InfoBalloon = "Info balloon for analysis 2")]
            public string Analysis2 => "analysis2";

            [DetectionPresentation(DetectionPresentationSection.Analysis, "Analysis 3", InfoBalloon = "Info balloon for analysis 3")]
            public DateTime Analysis3 => new DateTime(2012, 11, 12, 17, 22, 37);

            public string NoPresentation { get; set; } = "no show";

            [DetectionPredicate]
            public string OnlyPredicate { get; set; } = "only predicate";
        }

        private class TestDetectionNoSummaryChart : TestDetectionNoSummary
        {
            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
            public new double Value => 22.4;
        }

        private class TestDetectionTwoSummaryProperties : TestDetection
        {
            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Property, "Memory increased", InfoBalloon = "Memory increase on machine {MachineName}", Component = DetectionPresentationComponent.Summary)]
            public int MoreSummary => 7;
        }

        private class TestDetectionTwoSummaryCharts : TestDetection
        {
            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Chart, "Memory over the last 7 days", InfoBalloon = "Memory chart for machine {MachineName}, showing increase of {Value}", Component = DetectionPresentationComponent.Summary)]
            public string AnotherSummaryChart => "<another query>";
        }

        private class TestDetectionInvalidSummarySection : TestDetectionNoSummaryProperty
        {
            [DetectionPredicate]
            [DetectionPresentation(DetectionPresentationSection.Analysis, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = DetectionPresentationComponent.Details | DetectionPresentationComponent.Summary)]
            public new double Value => 22.4;
        }
    }
}