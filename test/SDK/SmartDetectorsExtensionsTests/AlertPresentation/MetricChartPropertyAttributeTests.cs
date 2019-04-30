//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AggregationType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.AggregationType;
    using ContractsAggregationType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.AggregationType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsThresholdType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ThresholdType;
    using DynamicThreshold = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.DynamicThreshold;
    using DynamicThresholdFailingPeriodsSettings = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.DynamicThresholdFailingPeriodsSettings;
    using StaticThreshold = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.StaticThreshold;
    using ThresholdType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ThresholdType;

    [TestClass]
    public class MetricChartPropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenMetricChartPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(1, contractsAlert.AlertProperties.Count);

            MetricChartAlertProperty alertProperty = (MetricChartAlertProperty)contractsAlert.AlertProperties[0];
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

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithInvalidPropertyThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertOnInvalidProperty>();
        }

        public class TestAlert : TestAlertBase
        {
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
        }

        public class TestAlertOnInvalidProperty : TestAlertBase
        {
            [MetricChartProperty("MetricChartDisplayName", Order = 8)]
            public string NotAMetricChart => "Opps";
        }
    }
}
