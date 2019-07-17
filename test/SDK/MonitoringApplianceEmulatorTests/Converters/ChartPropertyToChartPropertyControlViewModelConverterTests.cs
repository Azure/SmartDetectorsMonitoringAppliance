//-----------------------------------------------------------------------
// <copyright file="ChartPropertyToChartPropertyControlViewModelConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChartPropertyToChartPropertyControlViewModelConverterTests
    {
        [TestMethod]
        public void WhenConvertingChartAlertPropertyToChartPropertyControlViewModelThenTheResultIsAsExpected()
        {
            var chartPoints = new List<ChartPoint> { new ChartPoint(DateTime.Now, 8), new ChartPoint(DateTime.Now.AddDays(1), 6), new ChartPoint(DateTime.Now.AddDays(2), 4), new ChartPoint(DateTime.Now.AddDays(3), 14), new ChartPoint(DateTime.Now.AddDays(5), 10) };
            var chartAlertProperty = new ChartAlertProperty("propertyName", "displayName", 5, ChartType.LineChart, ChartAxisType.Date, ChartAxisType.Number, chartPoints);

            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();
            object result = converter.Convert(chartAlertProperty, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(ChartPropertyControlViewModel));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenConvertingIntegerToChartPropertyControlViewModelThenAnExceptionIsThrown()
        {
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();

            converter.Convert(12, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));
        }

        [TestMethod]
        public void WhenConvertingNullToChartPropertyControlViewModelThenNullIsReturned()
        {
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();
            object result = converter.Convert(null, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsNull(result);
        }

        [TestMethod]
        public void WhenConvertingDisconnectedObjectToChartPropertyControlViewModelThenSameObjectIsReturned()
        {
            DisconnectedItem disconnectedItem = new DisconnectedItem();

            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();
            object result = converter.Convert(disconnectedItem, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.AreEqual(disconnectedItem, result, "The conversion result should be the converted disconnected item object");
        }
    }
}