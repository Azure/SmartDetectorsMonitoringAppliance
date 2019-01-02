//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertPresentationTests
    {
        private const string SmartDetectorName = "smartDetectorName";

        [TestInitialize]
        public void TestInitialize()
        {
        }

#pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
        [TestMethod]
        public void WhenProcessingAlertThenTheContractsAlertIsCreatedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert(new TestAlert());
            Assert.IsTrue(contractsAlert.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(contractsAlert.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, contractsAlert.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SmartDetectorName, contractsAlert.SmartDetectorName, "Unexpected Smart Detector name");
            Assert.AreEqual("Test title", contractsAlert.Title, "Unexpected title");
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
            Assert.AreEqual(8, contractsAlert.Properties.Count, "Unexpected number of properties");
            VerifyProperty(contractsAlert.Properties, "Machine name", AlertPresentationSection.Property, "strongOne", "The machine on which the CPU had increased", 1);
            VerifyProperty(contractsAlert.Properties, "CPU over the last 7 days", AlertPresentationSection.Chart, "<the query>", "CPU chart for machine strongOne, showing increase of 22.4");
            VerifyProperty(contractsAlert.Properties, "CPU increased", AlertPresentationSection.Property, "22.4", "CPU increase on machine strongOne");
            VerifyProperty(contractsAlert.Properties, "Another query 1", AlertPresentationSection.AdditionalQuery, "<query1>", "Info balloon for another query 1");
            VerifyProperty(contractsAlert.Properties, "Another query 2", AlertPresentationSection.AdditionalQuery, "<query2>", "Info balloon for another query 2");
            VerifyProperty(contractsAlert.Properties, "Analysis 1", AlertPresentationSection.Analysis, "analysis1", "Info balloon for analysis 1");
            VerifyProperty(contractsAlert.Properties, "Analysis 2", AlertPresentationSection.Analysis, "analysis2", "Info balloon for analysis 2");
            VerifyProperty(contractsAlert.Properties, "Analysis 3", AlertPresentationSection.Analysis, new DateTime(2012, 11, 12, 17, 22, 37).ToString("u", CultureInfo.InvariantCulture), "Info balloon for analysis 3");
            Assert.AreEqual("no show", contractsAlert.RawProperties["NoPresentation"]);
            Assert.AreEqual(TelemetryDbType.LogAnalytics, contractsAlert.QueryRunInfo.Type, "Unexpected telemetry DB type");
            CollectionAssert.AreEqual(new[] { "resourceId1", "resourceId2" }, contractsAlert.QueryRunInfo.ResourceIds.ToArray(), "Unexpected resource IDs");
        }
#pragma warning restore CS0612 // Type or member is obsolete

        [TestMethod]
        public void WhenProcessingAlertWithoutSummaryChartThenNoExceptionIsThrown()
        {
            CreateContractsAlert(new TestAlertNoSummaryChart());
        }

        [TestMethod]
        public void WhenAlertsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var alert1 = new TestAlert();
            var alert2 = new TestAlert();
            alert2.NoPresentation += "X";

            var contractsAlert1 = CreateContractsAlert(alert1);

            // A non predicate property is different - correlation hash should be the same
            var contractsAlert2 = CreateContractsAlert(alert2);
            Assert.AreNotEqual(contractsAlert1.Id, contractsAlert2.Id);
            Assert.AreEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            alert2.OnlyPredicate += "X";
            contractsAlert2 = CreateContractsAlert(alert2);
            Assert.AreNotEqual(contractsAlert1.Id, contractsAlert2.Id);
            Assert.AreNotEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);
        }

        [TestMethod]
        public void WhenProcessingAlertWithoutQueriesAndNullRunInfoThenTheContractsAlertIsCreatedSuccessfully()
        {
            CreateContractsAlert(new TestAlertNoQueries(), nullQueryRunInfo: true);
        }

        [TestMethod]
        public void WhenProcessingAlertAndMetricClientWasUsedThenTheSignalTypeIsCorrent()
        {
            ContractsAlert contractsAlert = CreateContractsAlert(new TestAlert(), usedMetricClient: true);
            Assert.AreEqual(SignalType.Metric, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogClientWasUsedThenTheSignalTypeIsCorrent()
        {
            ContractsAlert contractsAlert = CreateContractsAlert(new TestAlert(), usedLogAnalysisClient: true);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogAndMetricClientsWereUsedThenTheSignalTypeIsCorrent()
        {
            ContractsAlert contractsAlert = CreateContractsAlert(new TestAlert(), usedLogAnalysisClient: true, usedMetricClient: true);
            Assert.AreEqual(SignalType.Multiple, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidAlertPresentationException))]
        public void WhenProcessingAlertWithQueriesAndNullRunInfoThenAnExceptionIsThrown()
        {
            CreateContractsAlert(new TestAlert(), nullQueryRunInfo: true);
        }

        private static ContractsAlert CreateContractsAlert(Alert alert, bool nullQueryRunInfo = false, bool usedLogAnalysisClient = false, bool usedMetricClient = false)
        {
            QueryRunInfo queryRunInfo = null;
            if (!nullQueryRunInfo)
            {
                queryRunInfo = new QueryRunInfo
                {
                    Type = alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights ? TelemetryDbType.ApplicationInsights : TelemetryDbType.LogAnalytics,
                    ResourceIds = new List<string> { "resourceId1", "resourceId2" }
                };
            }

            string resourceId = "resourceId";
            var request = new SmartDetectorAnalysisRequest
            {
                ResourceIds = new List<string>() { resourceId },
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromDays(1),
            };

            return alert.CreateContractsAlert(request, SmartDetectorName, queryRunInfo, usedLogAnalysisClient, usedMetricClient);
        }

#pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
        private static void VerifyProperty(List<AlertPropertyLegacy> properties, string name, AlertPresentationSection displayCategory, string value, string infoBalloon, byte order = byte.MaxValue)
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.IsNotNull(property, $"Property {name} not found");
            Assert.AreEqual(displayCategory.ToString(), property.DisplayCategory.ToString());
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(infoBalloon, property.InfoBalloon);
            Assert.AreEqual(order, property.Order);
        }
#pragma warning restore CS0612 // Type or member is obsolete

        public class TestAlertNoSummary : Alert
        {
            public TestAlertNoSummary()
                : base("Test title", default(ResourceIdentifier), DateTime.UtcNow)
            {
                this.Value = 22.4;
                this.MachineName = "strongOne";
            }

            [PredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public double Value { get; }

            [PredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "Machine name", Order = 1, InfoBalloon = "The machine on which the CPU had increased")]
            public string MachineName { get; }
        }

        public class TestAlertNoQueries : TestAlertNoSummary
        {
            public TestAlertNoQueries()
            {
                this.Value = 22.4;
            }

            [PredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value { get; }
        }

        public class TestAlertNoSummaryProperty : TestAlertNoSummary
        {
            public TestAlertNoSummaryProperty()
            {
                this.CpuChartQuery = "<the query>";
            }

            [AlertPresentationProperty(AlertPresentationSection.Chart, "CPU over the last 7 days", InfoBalloon = "CPU chart for machine {MachineName}, showing increase of {Value}")]
            public string CpuChartQuery { get; }
        }

        public class TestAlert : TestAlertNoSummaryProperty
        {
            public TestAlert()
            {
                this.Value = 22.4;
                this.Query1 = "<query1>";
                this.Query2 = "<query2>";
                this.Analysis1 = "analysis1";
                this.Analysis2 = "analysis2";
                this.Analysis3 = new DateTime(2012, 11, 12, 17, 22, 37);
                this.NoPresentation = "no show";
                this.OnlyPredicate = "only predicate";
            }

            [PredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value { get; }

            [AlertPresentationProperty(AlertPresentationSection.AdditionalQuery, "Another query 1", InfoBalloon = "Info balloon for another query 1")]
            public string Query1 { get; }

            [AlertPresentationProperty(AlertPresentationSection.AdditionalQuery, "Another query 2", InfoBalloon = "Info balloon for another query 2")]
            public string Query2 { get; }

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 1", InfoBalloon = "Info balloon for analysis 1")]
            public string Analysis1 { get; }

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 2", InfoBalloon = "Info balloon for analysis 2")]
            public string Analysis2 { get; }

            [AlertPresentationProperty(AlertPresentationSection.Analysis, "Analysis 3", InfoBalloon = "Info balloon for analysis 3")]
            public DateTime Analysis3 { get; }

            public string NoPresentation { get; set; }

            [PredicateProperty]
            public string OnlyPredicate { get; set; }
        }

        public class TestAlertNoSummaryChart : TestAlertNoSummary
        {
            public TestAlertNoSummaryChart()
            {
                this.Value = 22.4;
            }

            [PredicateProperty]
            [AlertPresentationProperty(AlertPresentationSection.Property, "CPU increased", InfoBalloon = "CPU increase on machine {MachineName}")]
            public new double Value { get; }
        }
    }
}