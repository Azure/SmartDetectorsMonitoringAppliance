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
        public void WhenConvertingTablePropertyThenResultIsTablePropertyControlViewModel()
        {
            var chartPoints = new List<ChartPoint> { new ChartPoint(DateTime.Now, 8), new ChartPoint(DateTime.Now.AddDays(1), 6), new ChartPoint(DateTime.Now.AddDays(2), 4), new ChartPoint(DateTime.Now.AddDays(3), 14), new ChartPoint(DateTime.Now.AddDays(5), 10) };
            var tableAlertProperty = new ChartAlertProperty("propertyName", "displayName", 5, ChartType.LineChart, ChartAxisType.Date, ChartAxisType.Number, chartPoints);
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();

            object result = converter.Convert(tableAlertProperty, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(ChartPropertyControlViewModel));
        }

        [TestMethod]
        public void WhenConvertingIntegerThenExceptionIsThrown()
        {
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();

            Exception thrownException = null;
            try
            {
                converter.Convert(12, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
        }

        [TestMethod]
        public void WhenConvertingNullThenNullIsReturned()
        {
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();

            object result = converter.Convert(null, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.IsNull(result);
        }

        [TestMethod]
        public void WhenConvertingDisconnectedObjectThenSameObjectIsReturned()
        {
            var converter = new ChartPropertyToChartPropertyControlViewModelConverter();

            DisconnectedItem disconnectedItem = new DisconnectedItem();

            object result = converter.Convert(disconnectedItem, typeof(ChartPropertyControlViewModel), null, new CultureInfo("en-us"));

            Assert.AreEqual(disconnectedItem, result, "The conversion result should be the converted disconnected item object");
        }
    }
}