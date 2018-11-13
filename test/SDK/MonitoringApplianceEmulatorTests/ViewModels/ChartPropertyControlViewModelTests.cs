//-----------------------------------------------------------------------
// <copyright file="ChartPropertyControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChartPropertyControlViewModelTests
    {
        private static readonly DateTime Now = DateTime.Now;

        private static readonly List<ChartPoint> DateTimeChartPoints = new List<ChartPoint>
        {
            new ChartPoint(Now, 8.0),
            new ChartPoint(Now.AddDays(1), 6.0),
            new ChartPoint(Now.AddDays(2), 4.0),
            new ChartPoint(Now.AddDays(3), 14.0),
            new ChartPoint(Now.AddDays(4), 10.0)
        };

        private static readonly List<ChartPoint> SingleDateTimeChartPoints = new List<ChartPoint>
        {
            new ChartPoint(Now, 8.0)
        };

        private static readonly List<ChartPoint> NumericChartPoints = new List<ChartPoint>
        {
            new ChartPoint(1.0, 8.0),
            new ChartPoint(2.0, 6.0),
            new ChartPoint(3.0, 4.0),
            new ChartPoint(4.0, 14.0),
            new ChartPoint(5.0, 10.0)
        };

        public delegate void ChartPointsAssertionDelegate(List<ChartPoint> expectedPoints, List<LiveCharts.ChartPoint> actualPoints);

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithDateTimeXAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(ChartType.LineChart, ChartAxisType.Date, DateTimeChartPoints, AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithSingleDateTimePointXAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(ChartType.LineChart, ChartAxisType.Date, SingleDateTimeChartPoints, AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithDateTimeXAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(ChartType.BarChart, ChartAxisType.Date, DateTimeChartPoints, AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithNumericXAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(ChartType.LineChart, ChartAxisType.Number, NumericChartPoints, AssertNumericChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithNumericXAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(ChartType.BarChart, ChartAxisType.Number, NumericChartPoints, AssertNumericChartPoints);
        }

        private static void AssertChartViewModel<T>(ChartType chartType, ChartAxisType xAxisType, List<ChartPoint> expectedChartPoints, ChartPointsAssertionDelegate pointsAssertionMethod)
            where T : Series
        {
            // Init
            var chartAlertProperty = new ChartAlertProperty(
                "propertyName",
                "displayName",
                1,
                chartType,
                xAxisType,
                ChartAxisType.Number,
                expectedChartPoints);

            // Act
            var chartPropertyControlViewModel = new ChartPropertyControlViewModel(chartAlertProperty);

            // Assert
            Assert.AreEqual("displayName", chartPropertyControlViewModel.Title, "Unexpected chart title");

            Assert.IsNotNull(chartPropertyControlViewModel, "View model is expected bo be defined");
            Assert.IsNotNull(chartPropertyControlViewModel.SeriesCollection[0], "Series is expected to be defined");

            T series = chartPropertyControlViewModel.SeriesCollection[0] as T;
            Assert.IsNotNull(series, $"Series is from type {chartPropertyControlViewModel.SeriesCollection[0].GetType()}, but expected to be from type {typeof(T)}");

            List<LiveCharts.ChartPoint> actualDataPoints = series.ChartPoints.ToList();
            pointsAssertionMethod(expectedChartPoints, actualDataPoints);
        }

        private static void AssertDateTimeChartPoints(List<ChartPoint> expectedChartPoints, List<LiveCharts.ChartPoint> actualDataPoints)
        {
            double xAxisFactor = actualDataPoints.Count > 1 ?
                Now.AddDays(1).Ticks - Now.Ticks :
                Now.Ticks;

            for (var i = 0; i < actualDataPoints.Count(); i++)
            {
                var expectedDataPoint = expectedChartPoints[i];
                var actualDataPoint = actualDataPoints[i];

                Assert.AreEqual((double)((DateTime)expectedDataPoint.X).Ticks / xAxisFactor, actualDataPoint.X, $"Unexpected X value for point in index: {i}");
                Assert.AreEqual(expectedDataPoint.Y, actualDataPoint.Y, $"Unexpected Y value for point in index: {i}");
            }
        }

        private static void AssertNumericChartPoints(List<ChartPoint> expectedChartPoints, List<LiveCharts.ChartPoint> actualDataPoints)
        {
            for (var i = 0; i < actualDataPoints.Count(); i++)
            {
                var expectedDataPoint = expectedChartPoints[i];
                var actualDataPoint = actualDataPoints[i];

                Assert.AreEqual(expectedDataPoint.X, actualDataPoint.X, $"Unexpected X value for point in index: {i}");
                Assert.AreEqual(expectedDataPoint.Y, actualDataPoint.Y, $"Unexpected Y value for point in index: {i}");
            }
        }
    }
}
