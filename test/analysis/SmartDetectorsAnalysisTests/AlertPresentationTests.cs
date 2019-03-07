//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTests.cs" company="Microsoft Corporation">
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
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using AggregationType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.AggregationType;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartType;
    using ContractsAggregationType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.AggregationType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartAxisType;
    using ContractsChartType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartType;
    using ContractsThresholdType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ThresholdType;
    using DynamicThreshold = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.DynamicThreshold;
    using DynamicThresholdFailingPeriodsSettings = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.DynamicThresholdFailingPeriodsSettings;
    using StaticThreshold = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.StaticThreshold;
    using ThresholdType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ThresholdType;

    [TestClass]
    public class AlertPresentationTests
    {
        private const string SmartDetectorName = "smartDetectorName";

        [TestMethod]
        public void WhenProcessingAlertThenTheContractsAlertIsCreatedCorrectly()
        {
            ContractsAlert contractsAlert = SetupContractsAlert(new PresentationTestAlert());
            Assert.IsTrue(contractsAlert.AnalysisTimestamp <= DateTime.UtcNow, "Unexpected analysis timestamp in the future");
            Assert.IsTrue(contractsAlert.AnalysisTimestamp >= DateTime.UtcNow.AddMinutes(-1), "Unexpected analysis timestamp - too back in the past");
            Assert.AreEqual(24 * 60, contractsAlert.AnalysisWindowSizeInMinutes, "Unexpected analysis window size");
            Assert.AreEqual(SmartDetectorName, contractsAlert.SmartDetectorName, "Unexpected Smart Detector name");
            Assert.AreEqual("AlertTitle", contractsAlert.Title, "Unexpected title");
            Assert.AreEqual(default(ResourceIdentifier).ToResourceId(), contractsAlert.ResourceId, "Unexpected ResourceId");
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
            Assert.AreEqual(24, contractsAlert.AlertProperties.Count, "Unexpected number of properties");

            // Verify raw alert properties
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "Predicate");
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "RawProperty");
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "AdditionalData_0_RawProperty");
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_0_RawProperty");
            VerifyPresentationTestAlertRawProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_1_RawProperty");

            // Verify displayed alert properties
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "LongTextPropertyName", "LongTextDisplayName", 0);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "UrlValue", "UrlDisplayName", 1);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "TextValue", "TextDisplayName", 2);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "KeyValue", "KeyValueDisplayName", 3);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "KeyValueWithHeaders", "KeyValueWithHeadersDisplayName", 4);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "Table", "TableDisplayName", 5);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "SingleColumnTable", "SingleColumnTableDisplayName", 6);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_0_Name1", "First name title", 7);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_0_Uri1", "First link", 8);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_Name2", "Second name title", 9);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_Uri2", "Second link", 10);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_0_Name1", "First name title", 11);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_0_Uri1", "First link", 12);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_1_Name1", "First name title", 13);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "AdditionalData_1_MoreData_1_Uri1", "First link", 14);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "MetricChart", "MetricChartDisplayName", 15);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "DateTimeValue", "DateTimeDisplayName", 16);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "DoubleValue", "DoubleDisplayName", 17);
            VerifyPresentationTestAlertDisplayedProperty(contractsAlert.AlertProperties, "DataPoints", "ChartDisplayName", 18);
        }

        [TestMethod]
        public void WhenAlertsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            var alert1 = new PresentationTestAlert();
            var alert2 = new PresentationTestAlert();

            var contractsAlert1 = SetupContractsAlert(alert1);

            // A non predicate property is different - correlation hash should be the same
            alert2.RawProperty++;
            var contractsAlert2 = SetupContractsAlert(alert2);
            Assert.AreEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);

            // A predicate property is different - correlation hash should be the different
            alert2 = new PresentationTestAlert("AlertTitle2");
            contractsAlert2 = SetupContractsAlert(alert2);
            Assert.AreNotEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);
        }

        [TestMethod]
        public void WhenProcessingAlertAndMetricClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = SetupContractsAlert(new PresentationTestAlert(), usedMetricClient: true);
            Assert.AreEqual(SignalType.Metric, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = SetupContractsAlert(new PresentationTestAlert(), usedLogAnalysisClient: true);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenProcessingAlertAndLogAndMetricClientsWereUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = SetupContractsAlert(new PresentationTestAlert(), usedLogAnalysisClient: true, usedMetricClient: true);
            Assert.AreEqual(SignalType.Multiple, contractsAlert.SignalType, "Unexpected signal type");
        }

        private static ContractsAlert SetupContractsAlert(Alert alert, bool usedLogAnalysisClient = false, bool usedMetricClient = false)
        {
            string resourceId = "resourceId";
            var request = new SmartDetectorAnalysisRequest
            {
                ResourceIds = new List<string>() { resourceId },
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromDays(1),
            };

            ContractsAlert contractsAlert = alert.CreateContractsAlert(request, SmartDetectorName, usedLogAnalysisClient, usedMetricClient);

            // Verify that all property names are unique
            var duplicates = contractsAlert.AlertProperties.GroupBy(x => x.PropertyName).Where(g => g.Count() > 1).ToList();
            if (duplicates.Count > 0)
            {
                Assert.Fail($"Duplicate alert properties found: {string.Join(", ", duplicates.Select(g => g.Key))}");
            }

            return contractsAlert;
        }

        private static void VerifyPresentationTestAlertDisplayedProperty(List<AlertProperty> properties, string propertyName, string displayName, byte order)
        {
            var property = properties.OfType<DisplayableAlertProperty>().SingleOrDefault(p => p.PropertyName == propertyName && p.Order == order);
            Assert.IsNotNull(property, $"Property {propertyName} not found");

            Assert.AreEqual(displayName, property.DisplayName);

            if (propertyName == "LongTextPropertyName")
            {
                Assert.AreEqual("LongTextValue", ((LongTextAlertProprety)property).Value);
            }
            else if (propertyName == "UrlValue")
            {
                Assert.AreEqual("<a href=\"https://www.bing.com/\" target=\"_blank\">LinkText1</a>", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "TextValue")
            {
                Assert.AreEqual("TextValue", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "DateTimeValue")
            {
                Assert.AreEqual("The time is: 2019-03-07 11", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "DoubleValue")
            {
                Assert.AreEqual("The area of the unit circle is 3, or 3.141593 to be exact", ((LongTextAlertProprety)property).Value);
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
                TableAlertProperty<Dictionary<string, string>> alertProperty = (TableAlertProperty<Dictionary<string, string>>)property;
                Assert.AreEqual(true, alertProperty.ShowHeaders);

                Assert.AreEqual(2, alertProperty.Values.Count);
                Assert.AreEqual(4, alertProperty.Values[0].Count);
                Assert.AreEqual("p11", alertProperty.Values[0]["Prop1"]);
                Assert.AreEqual("The value of Prop2 is p21", alertProperty.Values[0]["Prop2"]);
                Assert.AreEqual("p31", alertProperty.Values[0]["Prop3"]);
                Assert.AreEqual("<a href=\"http://microsoft.com/\" target=\"_blank\">Link for NDP1</a>", alertProperty.Values[0]["UriProp"]);

                Assert.AreEqual(4, alertProperty.Values[1].Count);
                Assert.AreEqual("p12", alertProperty.Values[1]["Prop1"]);
                Assert.AreEqual("The value of Prop2 is p22", alertProperty.Values[1]["Prop2"]);
                Assert.AreEqual("p32", alertProperty.Values[1]["Prop3"]);
                Assert.AreEqual("<a href=\"http://contoso.com/\" target=\"_blank\">Link for NDP2</a>", alertProperty.Values[1]["UriProp"]);

                Assert.AreEqual(4, alertProperty.Columns.Count);
                Assert.AreEqual("Prop1", alertProperty.Columns[0].PropertyName);
                Assert.AreEqual("First Prop", alertProperty.Columns[0].DisplayName);
                Assert.AreEqual("Prop2", alertProperty.Columns[1].PropertyName);
                Assert.AreEqual("Second Prop", alertProperty.Columns[1].DisplayName);
                Assert.AreEqual("UriProp", alertProperty.Columns[2].PropertyName);
                Assert.AreEqual("Uri Prop", alertProperty.Columns[2].DisplayName);
                Assert.AreEqual("Prop3", alertProperty.Columns[3].PropertyName);
                Assert.AreEqual("Third Prop, without order", alertProperty.Columns[3].DisplayName);
            }
            else if (propertyName == "SingleColumnTable")
            {
                TableAlertProperty<string> alertProperty = (TableAlertProperty<string>)property;
                Assert.AreEqual(false, alertProperty.ShowHeaders);

                Assert.AreEqual(3, alertProperty.Values.Count);
                Assert.AreEqual("value1", alertProperty.Values[0]);
                Assert.AreEqual("value2", alertProperty.Values[1]);
                Assert.AreEqual("value3", alertProperty.Values[2]);

                Assert.AreEqual(0, alertProperty.Columns.Count);
            }
            else if (propertyName.EndsWith("_Name1", StringComparison.InvariantCulture))
            {
                Assert.AreEqual("First name", ((TextAlertProperty)property).Value);
            }
            else if (propertyName.EndsWith("_Name2", StringComparison.InvariantCulture))
            {
                Assert.AreEqual("Second name", ((TextAlertProperty)property).Value);
            }
            else if (propertyName.EndsWith("_Uri1", StringComparison.InvariantCulture))
            {
                Assert.AreEqual("<a href=\"https://xkcd.com/\" target=\"_blank\">Link to data 1</a>", ((TextAlertProperty)property).Value);
            }
            else if (propertyName.EndsWith("_Uri2", StringComparison.InvariantCulture))
            {
                Assert.AreEqual("<a href=\"https://darwinawards.com/\" target=\"_blank\">Link to data 2</a>", ((TextAlertProperty)property).Value);
            }
            else if (propertyName == "MetricChart")
            {
                MetricChartAlertProperty alertProperty = (MetricChartAlertProperty)property;
                Assert.AreEqual(default(ResourceIdentifier).ToResourceId(), alertProperty.ResourceId);
                Assert.AreEqual("someMetric", alertProperty.MetricName);
                Assert.AreEqual("namespace", alertProperty.MetricNamespace);
                Assert.AreEqual(2, alertProperty.MetricDimensions.Count);
                Assert.AreEqual("val1", alertProperty.MetricDimensions["dim1"]);
                Assert.AreEqual("val2", alertProperty.MetricDimensions["dim2"]);
                Assert.AreEqual(new DateTime(1972, 6, 6), alertProperty.StartTimeUtc);
                Assert.AreEqual(new DateTime(1972, 6, 7), alertProperty.EndTimeUtc);
                Assert.AreEqual(TimeSpan.FromHours(1), alertProperty.TimeGrain);
                Assert.AreEqual(ContractsAggregationType.Average, alertProperty.AggregationType);
                Assert.AreEqual(ContractsThresholdType.LessThan, alertProperty.ThresholdType);
                Assert.AreEqual(0.1, alertProperty.StaticThreshold.LowerThreshold);
                Assert.AreEqual(0.5, alertProperty.StaticThreshold.UpperThreshold);
                Assert.AreEqual((uint)5, alertProperty.DynamicThreshold.FailingPeriodsSettings.ConsecutivePeriods);
                Assert.AreEqual((uint)3, alertProperty.DynamicThreshold.FailingPeriodsSettings.ConsecutiveViolations);
                Assert.AreEqual(DynamicThreshold.MediumSensitivity, alertProperty.DynamicThreshold.Sensitivity);
                Assert.AreEqual(new DateTime(1972, 6, 6), alertProperty.DynamicThreshold.IgnoreDataBefore);
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
            public PresentationTestAlert(string title = "AlertTitle", ResourceIdentifier resourceIdentifier = default(ResourceIdentifier))
                : base(title, resourceIdentifier)
            {
                this.RawProperty = 1;
            }

            [PredicateProperty]
            public string Predicate => this.Title;

            public int RawProperty { get; set; }

            [LongTextProperty("LongTextDisplayName", Order = 0, PropertyName = "LongTextPropertyName")]
            public string LongTextValue => "LongTextValue";

            [UrlFormatter("LinkText{RawProperty}")]
            [TextProperty("UrlDisplayName", Order = 1)]
            public Uri UrlValue => new Uri("https://www.bing.com");

            [TextProperty("TextDisplayName", Order = 2)]
            public string TextValue => "TextValue";

            [KeyValueProperty("KeyValueDisplayName", Order = 3)]
            public IDictionary<string, string> KeyValue => new Dictionary<string, string> { { "key1", "value1" } };

            [KeyValueProperty("KeyValueWithHeadersDisplayName", "Keys", "Values{RawProperty}", Order = 4)]
            public IDictionary<string, string> KeyValueWithHeaders => new Dictionary<string, string> { { "key1", "value1" } };

            [ChartProperty("ChartDisplayName", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis)]
            public List<ChartPoint> DataPoints => new List<ChartPoint>() { new ChartPoint(new DateTime(2018, 7, 9, 14, 31, 0, DateTimeKind.Utc), 5) };

            [MultiColumnTableProperty("TableDisplayName", Order = 5, ShowHeaders = true)]
            [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Test code, allowed")]
            public TableData[] Table => new[]
            {
                new TableData { Prop1 = "p11", Prop2 = "p21", Prop3 = "p31", UriProp = new Uri("http://microsoft.com"), NonDisplayProp = "NDP1" },
                new TableData { Prop1 = "p12", Prop2 = "p22", Prop3 = "p32", UriProp = new Uri("http://contoso.com"), NonDisplayProp = "NDP2" },
            };

            [SingleColumnTableProperty("SingleColumnTableDisplayName", Order = 6, ShowHeaders = false)]
            public List<string> SingleColumnTable => new List<string> { "value1", "value2", "value3" };

            [ListProperty(Order = 7)]
            public IList<object> AdditionalData => new List<object>()
            {
                new ListData1(),
                new ListData2()
            };

            [MetricChartProperty("MetricChartDisplayName", Order = 8)]
            public MetricChart MetricChart => new MetricChart("someMetric", TimeSpan.FromHours(1), AggregationType.Average)
            {
                ResourceId = default(ResourceIdentifier),
                MetricNamespace = "namespace",
                MetricDimensions =
                {
                    ["dim1"] = "val1",
                    ["dim2"] = "val2",
                },
                StartTimeUtc = new DateTime(1972, 6, 6),
                EndTimeUtc = new DateTime(1972, 6, 7),
                ThresholdType = ThresholdType.LessThan,
                StaticThreshold = new StaticThreshold
                {
                    LowerThreshold = 0.1,
                    UpperThreshold = 0.5,
                },
                DynamicThreshold = new DynamicThreshold(new DynamicThresholdFailingPeriodsSettings(5, 3), DynamicThreshold.MediumSensitivity)
                {
                    IgnoreDataBefore = new DateTime(1972, 6, 6),
                }
            };

            [TextProperty("DateTimeDisplayName", Order = 9, FormatString = "The time is: {0:yyyy-MM-dd HH}")]
            public DateTime DateTimeValue => new DateTime(2019, 3, 7, 11, 23, 47);

            [LongTextProperty("DoubleDisplayName", Order = 10, FormatString = "The area of the unit circle is {0:F0}, or {0:F6} to be exact")]
            public double DoubleValue => Math.PI;
        }

        public class TableData
        {
            [JsonProperty("prop3")]
            [TableColumn("Third Prop, without order")]
            public string Prop3 { get; set; }

            [TableColumn("Second Prop", Order = 2, FormatString = "The value of Prop2 is {0}")]
            public string Prop2 { get; set; }

            [UrlFormatter("Link for {NonDisplayProp}")]
            [TableColumn("Uri Prop", Order = 3)]
            public Uri UriProp { get; set; }

            // The properties are sorted like this to ensure that
            // the Order property is taken into account
            [JsonProperty("prop1")]
            [TableColumn("First Prop", Order = 1)]
            public string Prop1 { get; set; }

            public string NonDisplayProp { get; set; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        public class ListData1
        {
            [TextProperty("First name title", Order = 1)]
            public string Name1 => "First name";

            [UrlFormatter("Link to data 1")]
            [TextProperty("First link", Order = 2)]
            public Uri Uri1 => new Uri("https://xkcd.com/");

            public string RawProperty => "raw";
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
        public class ListData2
        {
            [TextProperty("Second name title", Order = 1)]
            public string Name2 => "Second name";

            [UrlFormatter("Link to data 2")]
            [TextProperty("Second link", Order = 2)]
            public Uri Uri2 => new Uri("https://darwinawards.com/");

            [ListProperty(Order = 3)]
            public IList<ListData1> MoreData => new List<ListData1>()
            {
                new ListData1(),
                new ListData1()
            };

            [ListProperty(Order = 4)]
            public IList<ListData1> EmptyList => new List<ListData1>();
        }
    }
}