//-----------------------------------------------------------------------
// <copyright file="AlertPresentationV2Tests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using AlertState = Microsoft.Azure.Monitoring.SmartDetectors.AlertState;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsAlertState = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertState;
    using ContractsChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartAxisType;
    using ContractsChartType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartType;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertPresentationV2Tests
    {
        private const string SmartDetectorName = "smartDetectorName";

        [TestMethod]
        public void WhenProcessingAlertWithV2PresentationThenTheContractsAlertIsCreatedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsV2Alert(new PresentationTestAlert());
            Assert.AreEqual(ContractsAlertState.Active, contractsAlert.State);
            Assert.IsTrue(contractsAlert.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(contractsAlert.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, contractsAlert.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SmartDetectorName, contractsAlert.SmartDetectorName, "Unexpected Smart Detector name");
            Assert.AreEqual("AlertTitle", contractsAlert.Title, "Unexpected title");
            Assert.AreEqual(default(ResourceIdentifier).ToResourceId(), contractsAlert.ResourceId, "Unexpected ResourceId");
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
            Assert.AreEqual(10, contractsAlert.AlertProperties.Count, "Unexpected number of properties");

            // Verify raw alert properties
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "Predicate");
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "RawProperty");

            // Verify displayed alert properties
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "LongTextPropertyName", "LongTextDisplayName", 0);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "UrlValue", "UrlDisplayName", 1);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "TextValue", "TextDisplayName", 2);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "KeyValue", "KeyValueDisplayName", 3);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "KeyValueWithHeaders", "KeyValueWithHeadersDisplayName", 4);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "Table", "TableDisplayName", 5);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "SingleColumnTable", "SingleColumnTableDisplayName", 6);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "DataPoints", "ChartDisplayName", byte.MaxValue);
        }

        [TestMethod]
        public void WhenProcessingAlertWithStateResolvedThenTheContractsAlertIsCreatedWithCorrectState()
        {
            ContractsAlert contractsAlert = CreateContractsV2Alert(new PresentationTestAlert("AlertTitle", default(ResourceIdentifier), AlertState.Resolved));
            Assert.AreEqual(ContractsAlertState.Resolved, contractsAlert.State);
        }

        [TestMethod]
        public void WhenAlertsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var alert1 = new PresentationTestAlert();
            var alert2 = new PresentationTestAlert(state: AlertState.Resolved);

            var contractsAlert1 = CreateContractsV2Alert(alert1);

            // A non predicate property is different - correlation hash should be the same
            var contractsAlert2 = CreateContractsV2Alert(alert2);
            Assert.AreNotEqual(contractsAlert1.Id, contractsAlert2.Id);
            Assert.AreEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            alert2 = new PresentationTestAlert("AlertTitle2");
            contractsAlert2 = CreateContractsV2Alert(alert2);
            Assert.AreNotEqual(contractsAlert1.Id, contractsAlert2.Id);
            Assert.AreNotEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);
        }

        [TestMethod]
        public void WhenProcessingAlertAndMetricClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = CreateContractsV2Alert(new PresentationTestAlert(), usedMetricClient: true);
            Assert.AreEqual(SignalType.Metric, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = CreateContractsV2Alert(new PresentationTestAlert(), usedLogAnalysisClient: true);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogAndMetricClientsWereUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = CreateContractsV2Alert(new PresentationTestAlert(), usedLogAnalysisClient: true, usedMetricClient: true);
            Assert.AreEqual(SignalType.Multiple, contractsAlert.SignalType, "Unexpected signal type");
        }

        private static ContractsAlert CreateContractsV2Alert(Alert alert, bool nullQueryRunInfo = false, bool usedLogAnalysisClient = false, bool usedMetricClient = false)
        {
            QueryRunInfo queryRunInfo = null;
            if (!nullQueryRunInfo)
            {
                queryRunInfo = new QueryRunInfo
                {
                    Type = alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights ? TelemetryDbType.ApplicationInsights : TelemetryDbType.LogAnalytics,
                    ResourceIds = new List<string>() { "resourceId1", "resourceId2" }
                };
            }

            string resourceId = "resourceId";
            var request = new SmartDetectorExecutionRequest
            {
                ResourceIds = new List<string>() { resourceId },
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromDays(1),
            };

            return alert.CreateContractsAlert(request, SmartDetectorName, queryRunInfo, usedLogAnalysisClient, usedMetricClient);
        }

        private static void VerifyPresentationTestAlertDisplayedProperty(List<AlertProperty> properties, string propertyName, string displayName, byte order)
        {
            var property = properties.SingleOrDefault(p => p.PropertyName == propertyName);
            Assert.IsNotNull(property, $"Property {propertyName} not found");

            Assert.IsInstanceOfType(property, typeof(DisplayableAlertProperty));
            Assert.AreEqual(order, ((DisplayableAlertProperty)property).Order);
            Assert.AreEqual(displayName, ((DisplayableAlertProperty)property).DisplayName);

            if (propertyName == "LongTextPropertyName")
            {
                Assert.AreEqual("LongTextValue", ((LongTextAlertProprety)property).Value);
            }
            else if (propertyName == "UrlValue")
            {
                Assert.AreEqual("<a href=\"https://www.bing.com/\">LinkText1</a>", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "TextValue")
            {
                Assert.AreEqual("TextValue", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "KeyValue")
            {
                KeyValueAlertProperty alertProperty = (KeyValueAlertProperty)property;
                Assert.AreEqual("value1", alertProperty.Value["key1"]);
                Assert.AreEqual(false, alertProperty.ShowHeaders);
            }
            else if (propertyName == "KeyValueWithHeaders")
            {
                KeyValueAlertProperty alertProperty = (KeyValueAlertProperty)property;
                Assert.AreEqual("value1", alertProperty.Value["key1"]);
                Assert.AreEqual(true, alertProperty.ShowHeaders);
                Assert.AreEqual("Keys", alertProperty.KeyHeaderName);
                Assert.AreEqual("Values1", alertProperty.ValueHeaderName);
            }
            else if (propertyName == "DataPoints")
            {
                ChartAlertProperty alertProperty = (ChartAlertProperty)property;

                Assert.AreEqual(1, alertProperty.DataPoints.Count);
                Assert.AreEqual(new DateTime(2018, 7, 9, 14, 31, 0, DateTimeKind.Utc), alertProperty.DataPoints[0].X);
                Assert.AreEqual(5, alertProperty.DataPoints[0].Y);

                Assert.AreEqual(ContractsChartType.LineChart, alertProperty.ChartType);
                Assert.AreEqual(ContractsChartAxisType.Date, alertProperty.XAxisType);
                Assert.AreEqual(ContractsChartAxisType.Number, alertProperty.YAxisType);
            }
            else if (propertyName == "Table")
            {
                TableAlertProperty alertProperty = (TableAlertProperty)property;
                Assert.AreEqual(true, alertProperty.ShowHeaders);

                Assert.AreEqual(2, alertProperty.Values.Count);
                Assert.IsInstanceOfType(alertProperty.Values[0], typeof(TableData));
                Assert.AreEqual("p11", ((TableData)alertProperty.Values[0]).Prop1);
                Assert.AreEqual("p21", ((TableData)alertProperty.Values[0]).Prop2);
                Assert.AreEqual("NDP1", ((TableData)alertProperty.Values[0]).NonDisplayProp);

                Assert.IsInstanceOfType(alertProperty.Values[1], typeof(TableData));
                Assert.AreEqual("p12", ((TableData)alertProperty.Values[1]).Prop1);
                Assert.AreEqual("p22", ((TableData)alertProperty.Values[1]).Prop2);
                Assert.AreEqual("NDP2", ((TableData)alertProperty.Values[1]).NonDisplayProp);

                Assert.AreEqual(2, alertProperty.Columns.Count);
                Assert.AreEqual("prop1", alertProperty.Columns[0].PropertyName);
                Assert.AreEqual("First Prop", alertProperty.Columns[0].DisplayName);
                Assert.AreEqual("Prop2", alertProperty.Columns[1].PropertyName);
                Assert.AreEqual("Second Prop", alertProperty.Columns[1].DisplayName);
            }
            else if (propertyName == "SingleColumnTable")
            {
                TableAlertProperty alertProperty = (TableAlertProperty)property;
                Assert.AreEqual(false, alertProperty.ShowHeaders);

                Assert.AreEqual(3, alertProperty.Values.Count);
                Assert.AreEqual("value1", alertProperty.Values[0]);
                Assert.AreEqual("value2", alertProperty.Values[1]);
                Assert.AreEqual("value3", alertProperty.Values[2]);

                Assert.AreEqual(0, alertProperty.Columns.Count);
            }
            else
            {
                Assert.Fail($"Unknown property '{propertyName}'");
            }
        }

        private static void VerifyPresentationTestAlertRawProperty(List<AlertProperty> properties, string propertyName)
        {
            var property = properties.SingleOrDefault(p => p.PropertyName == propertyName);
            Assert.IsNotNull(property, $"Property {propertyName} not found");
            Assert.IsInstanceOfType(property, typeof(RawAlertProperty));
            if (propertyName == "Predicate")
            {
                Assert.AreEqual("AlertTitle", ((RawAlertProperty)property).Value);
            }
            else if (propertyName == "RawProperty")
            {
                Assert.AreEqual(1, ((RawAlertProperty)property).Value);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        public class PresentationTestAlert : Alert
        {
            public PresentationTestAlert(string title = "AlertTitle", ResourceIdentifier resourceIdentifier = default(ResourceIdentifier), AlertState state = AlertState.Active)
                : base(title, resourceIdentifier, state)
            {
            }

            [AlertPredicateProperty]
            public string Predicate => this.Title;

            public int RawProperty => 1;

            [AlertPresentationLongTextAttribute("LongTextDisplayName", Order = 0, PropertyName = "LongTextPropertyName")]
            public string LongTextValue => "LongTextValue";

            [AlertPresentationUrl("UrlDisplayName", "LinkText{RawProperty}", Order = 1)]
            public Uri UrlValue => new Uri("https://www.bing.com");

            [AlertPresentationText("TextDisplayName", Order = 2)]
            public string TextValue => "TextValue";

            [AlertPresentationKeyValue("KeyValueDisplayName", Order = 3)]
            public IDictionary<string, string> KeyValue => new Dictionary<string, string> { { "key1", "value1" } };

            [AlertPresentationKeyValue("KeyValueWithHeadersDisplayName", "Keys", "Values{RawProperty}", Order = 4)]
            public IDictionary<string, string> KeyValueWithHeaders => new Dictionary<string, string> { { "key1", "value1" } };

            [AlertPresentationChart("ChartDisplayName", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis)]
            public List<ChartPoint> DataPoints => new List<ChartPoint>() { new ChartPoint(new DateTime(2018, 7, 9, 14, 31, 0, DateTimeKind.Utc), 5) };

            [AlertPresentationTable("TableDisplayName", Order = 5, ShowHeaders = true)]
            [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Test code, allowed")]
            public TableData[] Table => new TableData[]
            {
                new TableData { Prop1 = "p11", Prop2 = "p21", NonDisplayProp = "NDP1" },
                new TableData { Prop1 = "p12", Prop2 = "p22", NonDisplayProp = "NDP2" },
            };

            [AlertPresentationSingleColumnTable("SingleColumnTableDisplayName", Order = 6, ShowHeaders = false)]
            public List<string> SingleColumnTable => new List<string> { "value1", "value2", "value3" };
        }

        public class TableData
        {
            [JsonProperty("prop1")]
            [AlertPresentationTableColumn("First Prop")]
            public string Prop1 { get; set; }

            [AlertPresentationTableColumn("Second Prop")]
            public string Prop2 { get; set; }

            public string NonDisplayProp { get; set; }
        }
    }
}