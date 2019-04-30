//-----------------------------------------------------------------------
// <copyright file="ChartPropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartAxisType;
    using ContractsChartType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartType;

    [TestClass]
    public class ChartPropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenChartPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            ChartReferenceAlertProperty chartReferenceAlertProperty = (ChartReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("ChartReference", chartReferenceAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Chart, chartReferenceAlertProperty.Type);
            Assert.AreEqual("ChartReferenceDisplayName", chartReferenceAlertProperty.DisplayName);
            Assert.AreEqual(0, chartReferenceAlertProperty.Order);
            Assert.AreEqual(ContractsChartType.BarChart, chartReferenceAlertProperty.ChartType);
            Assert.AreEqual(ContractsChartAxisType.String, chartReferenceAlertProperty.XAxisType);
            Assert.AreEqual(ContractsChartAxisType.Percentage, chartReferenceAlertProperty.YAxisType);
            Assert.AreEqual("chartReferencePath", chartReferenceAlertProperty.ReferencePath);

            propertyIndex++;
            ChartAlertProperty chartAlertProperty = (ChartAlertProperty)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("DataPoints", chartAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Chart, chartAlertProperty.Type);
            Assert.AreEqual("ChartDisplayName", chartAlertProperty.DisplayName);
            Assert.AreEqual(1, chartAlertProperty.Order);
            Assert.AreEqual(ContractsChartType.LineChart, chartAlertProperty.ChartType);
            Assert.AreEqual(ContractsChartAxisType.Date, chartAlertProperty.XAxisType);
            Assert.AreEqual(ContractsChartAxisType.Number, chartAlertProperty.YAxisType);
            Assert.AreEqual(1, chartAlertProperty.DataPoints.Count);
            Assert.AreEqual(new DateTime(2018, 7, 9, 14, 31, 0, DateTimeKind.Utc), chartAlertProperty.DataPoints[0].X);
            Assert.AreEqual(5, chartAlertProperty.DataPoints[0].Y);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithInvalidChartPropertyThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithInvalidChart>();
        }

        public class TestAlert : TestAlertBase
        {
            [ChartProperty("ChartDisplayName", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis)]
            public List<ChartPoint> DataPoints => new List<ChartPoint>() { new ChartPoint(new DateTime(2018, 7, 9, 14, 31, 0, DateTimeKind.Utc), 5) };

            [ChartProperty("ChartReferenceDisplayName", ChartType.BarChart, ChartAxisType.StringAxis, ChartAxisType.PercentageAxis)]
            public PropertyReference ChartReference => new PropertyReference("chartReferencePath");
        }

        public class TestAlertWithInvalidChart : TestAlertBase
        {
            [ChartProperty("ChartDisplayName", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis)]
            public List<string> DataPoints => new List<string>() { "Oops" };
        }
    }
}
