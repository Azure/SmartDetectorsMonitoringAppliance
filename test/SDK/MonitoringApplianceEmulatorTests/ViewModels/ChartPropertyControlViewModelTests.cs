//-----------------------------------------------------------------------
// <copyright file="ChartPropertyControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

        private static readonly Func<double, string> PercentageYFormatter = value => $"{value.ToString(CultureInfo.InvariantCulture)}%";

        private static readonly Func<double, string> NumberFormatter = value => value.ToString(CultureInfo.InvariantCulture);

        private static Func<double, string> dateXFormatter;

        public delegate void ChartPointsAssertionDelegate(List<ChartPoint> expectedPoints, List<LiveCharts.ChartPoint> actualPoints);

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithDateTimeXAxisTypeAndWithNumericYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Number,
                expectedChartPoints: DateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithDateTimeXAxisTypeAndWithPercentageYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Percentage,
                expectedChartPoints: DateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithSingleDateTimePointXAxisTypeAndWithNumericYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Number,
                expectedChartPoints: SingleDateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithSingleDateTimePointXAxisTypeAndWithPercentageYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Percentage,
                expectedChartPoints: SingleDateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithDateTimeXAxisTypeAndWithNumericYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(
                chartType: ChartType.BarChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Number,
                expectedChartPoints: DateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithDateTimeXAxisTypeAndWithPercentageYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(
                chartType: ChartType.BarChart,
                xAxisType: ChartAxisType.Date,
                yAxisType: ChartAxisType.Percentage,
                expectedChartPoints: DateTimeChartPoints,
                pointsAssertionMethod: AssertDateTimeChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithNumericXAxisTypeAndWithNumericYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Number,
                yAxisType: ChartAxisType.Number,
                expectedChartPoints: NumericChartPoints,
                pointsAssertionMethod: AssertNumericChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForLineChartWithNumericXAxisTypeAndWithPercentageYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<LineSeries>(
                chartType: ChartType.LineChart,
                xAxisType: ChartAxisType.Number,
                yAxisType: ChartAxisType.Percentage,
                expectedChartPoints: NumericChartPoints,
                pointsAssertionMethod: AssertNumericChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithNumericXAxisTypeAndWithNumericYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(
                chartType: ChartType.BarChart,
                xAxisType: ChartAxisType.Number,
                yAxisType: ChartAxisType.Number,
                expectedChartPoints: NumericChartPoints,
                pointsAssertionMethod: AssertNumericChartPoints);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelForBarChartWithNumericXAxisTypeAndWithPercentageYAxisTypeThenItWasInitializedCorrectly()
        {
            AssertChartViewModel<ColumnSeries>(
                chartType: ChartType.BarChart,
                xAxisType: ChartAxisType.Number,
                yAxisType: ChartAxisType.Percentage,
                expectedChartPoints: NumericChartPoints,
                pointsAssertionMethod: AssertNumericChartPoints);
        }

        private static void AssertChartViewModel<T>(ChartType chartType, ChartAxisType xAxisType, ChartAxisType yAxisType, List<ChartPoint> expectedChartPoints, ChartPointsAssertionDelegate pointsAssertionMethod)
            where T : Series
        {
            // Init
            var chartAlertProperty = new ChartAlertProperty(
                "propertyName",
                "displayName",
                1,
                chartType,
                xAxisType,
                yAxisType,
                expectedChartPoints);

            // Act
            var chartPropertyControlViewModel = new ChartPropertyControlViewModel(chartAlertProperty);

            // Assert
            Assert.AreEqual("displayName", chartPropertyControlViewModel.Title, "Unexpected chart title");

            Assert.IsNotNull(chartPropertyControlViewModel, "View model is expected to be defined");
            Assert.IsNotNull(chartPropertyControlViewModel.SeriesCollection[0], "Series is expected to be defined");

            T series = chartPropertyControlViewModel.SeriesCollection[0] as T;
            Assert.IsNotNull(series, $"Series is from type {chartPropertyControlViewModel.SeriesCollection[0].GetType()}, but expected to be from type {typeof(T)}");

            List<LiveCharts.ChartPoint> actualDataPoints = series.ChartPoints.ToList();
            pointsAssertionMethod(expectedChartPoints, actualDataPoints);

            FormatterAssertMethod(chartPropertyControlViewModel, xAxisType, yAxisType, expectedChartPoints);
        }

        private static void FormatterAssertMethod(ChartPropertyControlViewModel chartPropertyControlViewModel, ChartAxisType xAxisType, ChartAxisType yAxisType, List<ChartPoint> expectedChartPoints)
        {
            // Assert Y axis formatter
            for (var i = 0; i < expectedChartPoints.Count(); i++)
            {
                if (yAxisType == ChartAxisType.Percentage)
                {
                    Assert.AreEqual(
                        chartPropertyControlViewModel.YAxisFormatter((double)expectedChartPoints[i].Y),
                        PercentageYFormatter((double)expectedChartPoints[i].Y));
                }
                else
                {
                    Assert.AreEqual(
                        chartPropertyControlViewModel.YAxisFormatter((double)expectedChartPoints[i].Y),
                        NumberFormatter((double)expectedChartPoints[i].Y));
                }
            }

            // Assert X axis formatter
            for (var i = 0; i < expectedChartPoints.Count(); i++)
            {
                if (xAxisType == ChartAxisType.Number)
                {
                    Assert.AreEqual(
                        chartPropertyControlViewModel.XAxisFormatter((double)expectedChartPoints[i].X),
                        NumberFormatter((double)expectedChartPoints[i].X));
                }
                else
                {
                    double xAxisFactor = expectedChartPoints.Count > 1 ?
                        Now.AddDays(1).Ticks - Now.Ticks :
                        Now.Ticks;
                    dateXFormatter = value => new DateTime((long)(value * xAxisFactor)).ToString(CultureInfo.InvariantCulture);

                    Assert.AreEqual(
                        chartPropertyControlViewModel.XAxisFormatter((double)((DateTime)expectedChartPoints[i].X).Ticks / xAxisFactor),
                        dateXFormatter((double)((DateTime)expectedChartPoints[i].X).Ticks / xAxisFactor));
                }
            }
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
