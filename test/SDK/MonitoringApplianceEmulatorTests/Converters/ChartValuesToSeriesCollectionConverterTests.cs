//-----------------------------------------------------------------------
// <copyright file="ChartValuesToSeriesCollectionConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System;
    using System.Globalization;
    using LiveCharts;
    using LiveCharts.Defaults;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ChartValuesToSeriesCollectionConverterTests
    {
        [TestMethod]
        public void WhenConvertingChartValuesToSeriesCollectionThenTheResultIsAsExpected()
        {
            ChartValues<DateTimePoint> values = new ChartValues<DateTimePoint>();
            values.Add(new DateTimePoint(new DateTime(2019, 4, 4, 10, 0, 0), 7.6));
            values.Add(new DateTimePoint(new DateTime(2019, 4, 4, 11, 0, 0), 3.43424));
            values.Add(new DateTimePoint(new DateTime(2019, 4, 4, 12, 0, 0), 100));

            var converter = new ChartValuesToSeriesCollectionConverter();
            object result = converter.Convert(values, typeof(ChartValues<DateTimePoint>), null, new CultureInfo("en-us"));

            var seriesCollection = result as SeriesCollection;
            Assert.IsNotNull(seriesCollection);
            Assert.AreEqual(1, seriesCollection.Count);
            var series = seriesCollection[0];
            CollectionAssert.AreEqual(values, series.ActualValues);
        }

        [TestMethod]
        public void WhenConvertingInvalidTypeTheResultIsNull()
        {
            var converter = new ChartValuesToSeriesCollectionConverter();
            object result = converter.Convert(1, typeof(ChartValues<DateTimePoint>), null, new CultureInfo("en-us"));

            Assert.IsNull(result);
        }
    }
}