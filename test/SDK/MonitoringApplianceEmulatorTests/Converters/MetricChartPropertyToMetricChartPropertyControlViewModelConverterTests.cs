//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyToMetricChartPropertyControlViewModelConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System;
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MetricChartPropertyToMetricChartPropertyControlViewModelConverterTests
    {
        private Mock<IInternalAnalysisServicesFactory> analysisServicesFactoryMock;
        private Mock<ITracer> tracerMock;
        private MetricChartPropertyToMetricChartPropertyControlViewModelConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            this.analysisServicesFactoryMock = new Mock<IInternalAnalysisServicesFactory>();
            this.tracerMock = new Mock<ITracer>();
            this.converter = new MetricChartPropertyToMetricChartPropertyControlViewModelConverter(this.analysisServicesFactoryMock.Object, this.tracerMock.Object);
        }

        [TestMethod]
        public void WhenConvertingMetricChartAlertPropertyToMetricChartPropertyControlViewModelThenTheResultIsAsExpected()
        {
            var metricChartAlertProperty = new MetricChartAlertProperty("propertyName", "displayName", 5, "metric1", TimeSpan.FromMinutes(15), AggregationType.Maximum);

            object result = this.converter.Convert(metricChartAlertProperty, typeof(MetricChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(MetricChartPropertyControlViewModel));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenConvertingIntegerToMetricChartPropertyControlViewModelThenExceptionIsThrown()
        {
            this.converter.Convert(12, typeof(MetricChartPropertyControlViewModel), null, new CultureInfo("en-us"));
        }

        [TestMethod]
        public void WhenConvertingNullToMetricChartPropertyControlViewModelThenNullIsReturned()
        {
            object result = this.converter.Convert(null, typeof(MetricChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsNull(result);
        }

        [TestMethod]
        public void WhenConvertingDisconnectedObjectToMetricChartPropertyControlViewModelThenSameObjectIsReturned()
        {
            DisconnectedItem disconnectedItem = new DisconnectedItem();

            object result = this.converter.Convert(disconnectedItem, typeof(MetricChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.AreEqual(disconnectedItem, result, "The conversion result should be the converted disconnected item object");
        }
    }
}