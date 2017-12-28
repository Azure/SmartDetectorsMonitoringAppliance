//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItemPresentationTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartSignalResultItemPresentationTests
    {
        private const string SignalName = "signalName";

        private Mock<IAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();
            this.azureResourceManagerClientMock
                .Setup(x => x.GetResourceId(It.IsAny<ResourceIdentifier>()))
                .Returns((ResourceIdentifier resourceIdentifier) => resourceIdentifier.ResourceName);
        }

        [TestMethod]
        public void WhenProcessingSmartSignalResultItemThenThePresentationIsCreatedCorrectly()
        {
            DateTime lastExecutionTime = DateTime.Now.Date.AddDays(-1);
            string resourceId = "resourceId";
            var request = new SmartSignalRequest(new List<string>() { resourceId }, "signalId", lastExecutionTime, TimeSpan.FromDays(1), new SmartSignalSettings());
            var signalResultItem = new TestResultItem();
            var presentation = SmartSignalResultItemPresentation.CreateFromResultItem(request, SignalName, signalResultItem, this.azureResourceManagerClientMock.Object);
            Assert.IsTrue(presentation.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(presentation.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, presentation.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SignalName, presentation.SignalName, "Unexpected signal name");
            Assert.AreEqual("Test title", presentation.Title, "Unexpected title");
            Assert.AreEqual("<the query>", presentation.Summary.Chart.Value, "Unexpected chart query");
            Assert.AreEqual(8, presentation.Properties.Count, "Unexpected number of properties");
            this.VerifyProperty(presentation.Properties, "Machine name", ResultItemPresentationSection.Property, "strongOne", "The machine on which the CPU had increased");
            this.VerifyProperty(presentation.Properties, "CPU over the last 7 days", ResultItemPresentationSection.Chart, "<the query>", "CPU chart for machine strongOne, showing increase of 22.4");
            this.VerifyProperty(presentation.Properties, "CPU increased", ResultItemPresentationSection.Property, "22.4", "CPU increase on machine strongOne");
            this.VerifyProperty(presentation.Properties, "Another query 1", ResultItemPresentationSection.AdditionalQuery, "<query1>", "Info balloon for another query 1");
            this.VerifyProperty(presentation.Properties, "Another query 2", ResultItemPresentationSection.AdditionalQuery, "<query2>", "Info balloon for another query 2");
            this.VerifyProperty(presentation.Properties, "Analysis 1", ResultItemPresentationSection.Analysis, "analysis1", "Info balloon for analysis 1");
            this.VerifyProperty(presentation.Properties, "Analysis 2", ResultItemPresentationSection.Analysis, "analysis2", "Info balloon for analysis 2");
            this.VerifyProperty(presentation.Properties, "Analysis 3", ResultItemPresentationSection.Analysis, (new DateTime(2012, 11, 12, 17, 22, 37)).ToString("u"), "Info balloon for analysis 3");
            Assert.AreEqual("no show", presentation.RawProperties["NoPresentation"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartSignalResultItemPresentationException))]
        public void WhenProcessingSmartSignalResultItemWithoutSummaryPropertyThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemNoSummary());
        }

        [TestMethod]
        public void WhenProcessingSmartSignalResultItemWithoutSummaryChartThenNoExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemNoSummaryChart());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartSignalResultItemPresentationException))]
        public void WhenProcessingSmartSignalResultItemWithTwoSummaryPropertiesThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemTwoSummaryProperties());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartSignalResultItemPresentationException))]
        public void WhenProcessingSmartSignalResultItemWithTwoSummaryChartsThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemTwoSummaryCharts());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartSignalResultItemPresentationException))]
        public void WhenProcessingSmartSignalResultItemWithSummaryInWrongSectionThenAnExceptionIsThrown()
        {
            this.CreatePresentation(new TestResultItemInvalidSummarySection());
        }

        [TestMethod]
        public void WhenSmartSignalResultItemsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var resultItem1 = new TestResultItem();
            var resultItem2 = new TestResultItem();
            resultItem2.NoPresentation += "X";

            var presentation1 = this.CreatePresentation(resultItem1);

            // A non predicate property is different - correlation hash should be the same
            var presentation2 = this.CreatePresentation(resultItem2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            resultItem2.OnlyPredicate += "X";
            presentation2 = this.CreatePresentation(resultItem2);
            Assert.AreNotEqual(presentation1.Id, presentation2.Id);
            Assert.AreNotEqual(presentation1.CorrelationHash, presentation2.CorrelationHash);
        }

        private SmartSignalResultItemPresentation CreatePresentation(SmartSignalResultItem resultItem)
        {
            DateTime lastExecutionTime = DateTime.Now.Date.AddDays(-1);
            string resourceId = "resourceId";
            var request = new SmartSignalRequest(new List<string>() { resourceId }, "signalId", lastExecutionTime, TimeSpan.FromDays(1), new SmartSignalSettings());
            return SmartSignalResultItemPresentation.CreateFromResultItem(request, SignalName, resultItem, this.azureResourceManagerClientMock.Object);
        }

        private void VerifyProperty(List<SmartSignalResultItemPresentationProperty> properties, string name, ResultItemPresentationSection displayCategory, string value, string infoBalloon)
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.IsNotNull(property, $"Property {name} not found");
            Assert.AreEqual(displayCategory, property.DisplayCategory);
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(infoBalloon, property.InfoBalloon);
        }

        private class TestResultItemNoSummary : SmartSignalResultItem
        {
            public TestResultItemNoSummary() : base("Test title", default(ResourceIdentifier))
            {
            }

            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = ResultItemPresentationComponent.Details)]
            public double Value => 22.4;

            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Property, "Machine name", InfoBalloon = "The machine on which the CPU had increased")]
            public string MachineName => "strongOne";
        }

        private class TestResultItemNoSummaryProperty : TestResultItemNoSummary
        {
            [ResultItemPresentation(ResultItemPresentationSection.Chart, "CPU over the last 7 days", InfoBalloon = "CPU chart for machine {MachineName}, showing increase of {Value}", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
            public string CpuChartQuery => "<the query>";
        }

        private class TestResultItem : TestResultItemNoSummaryProperty
        {
            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
            public new double Value => 22.4;

            [ResultItemPresentation(ResultItemPresentationSection.AdditionalQuery, "Another query 1", InfoBalloon = "Info balloon for another query 1")]
            public string Query1 => "<query1>";

            [ResultItemPresentation(ResultItemPresentationSection.AdditionalQuery, "Another query 2", InfoBalloon = "Info balloon for another query 2")]
            public string Query2 => "<query2>";

            [ResultItemPresentation(ResultItemPresentationSection.Analysis, "Analysis 1", InfoBalloon = "Info balloon for analysis 1")]
            public string Analysis1 => "analysis1";

            [ResultItemPresentation(ResultItemPresentationSection.Analysis, "Analysis 2", InfoBalloon = "Info balloon for analysis 2")]
            public string Analysis2 => "analysis2";

            [ResultItemPresentation(ResultItemPresentationSection.Analysis, "Analysis 3", InfoBalloon = "Info balloon for analysis 3")]
            public DateTime Analysis3 => new DateTime(2012, 11, 12, 17, 22, 37);

            public string NoPresentation { get; set; } = "no show";

            [ResultItemPredicate]
            public string OnlyPredicate { get; set; } = "only predicate";
        }

        private class TestResultItemNoSummaryChart : TestResultItemNoSummary
        {
            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
            public new double Value => 22.4;
        }

        private class TestResultItemTwoSummaryProperties : TestResultItem
        {
            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Property, "Memory increased", InfoBalloon = "Memory increase on machine {MachineName}", Component = ResultItemPresentationComponent.Summary)]
            public int MoreSummary => 7;
        }

        private class TestResultItemTwoSummaryCharts : TestResultItem
        {
            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Chart, "Memory over the last 7 days", InfoBalloon = "Memory chart for machine {MachineName}, showing increase of {Value}", Component = ResultItemPresentationComponent.Summary)]
            public string AnotherSummaryChart => "<another query>";
        }

        private class TestResultItemInvalidSummarySection : TestResultItemNoSummaryProperty
        {
            [ResultItemPredicate]
            [ResultItemPresentation(ResultItemPresentationSection.Analysis, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}", Component = ResultItemPresentationComponent.Details | ResultItemPresentationComponent.Summary)]
            public new double Value => 22.4;
        }
    }
}