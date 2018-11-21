//-----------------------------------------------------------------------
// <copyright file="ChartPropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// The view model class for the <see cref="ChartPropertyControl"/> control.
    /// </summary>
    public class ChartPropertyControlViewModel : ObservableObject
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;

        private SeriesCollection seriesCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="chartAlertProperty">The chart alert property that should be displayed.</param>
        public ChartPropertyControlViewModel(ChartAlertProperty chartAlertProperty)
        {
            this.Title = chartAlertProperty.DisplayName;

            if (chartAlertProperty.YAxisType != ChartAxisType.Number)
            {
                throw new InvalidOperationException($"Charts with Y axis type other than {ChartAxisType.Number} are not supported.");
            }

            switch (chartAlertProperty.XAxisType)
            {
                case ChartAxisType.Date:
                    this.InitializeDateTimeXAxisChart(chartAlertProperty);
                    break;
                case ChartAxisType.Number:
                    this.InitializeNumberXAxisChart(chartAlertProperty);
                    break;
                case ChartAxisType.String:
                    throw new NotImplementedException($"Charts with {ChartAxisType.String} X axis type will be supported soon.");
                default:
                    throw new InvalidOperationException($"Charts with X axis type {chartAlertProperty.XAxisType} are not supported.");
            }
        }

        /// <summary>
        /// Gets the series collection.
        /// </summary>
        public SeriesCollection SeriesCollection
        {
            get
            {
                return this.seriesCollection;
            }

            private set
            {
                this.seriesCollection = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the series collection.
        /// </summary>
        public Func<double, string> XAxisFormatter { get; set; }

        /// <summary>
        /// Gets the chart title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Converts a point's coordinate value to double.
        /// </summary>
        /// <param name="coordinateValue">The chart alert property that should be displayed.</param>
        /// <param name="coordinateName">The coordinate name (e.g. X, Y).</param>
        /// <returns>The coordinate value as double</returns>
        private static double ConvertCoordinateValueToDouble(object coordinateValue, string coordinateName)
        {
            try
            {
                return Convert.ToDouble(coordinateValue, CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is InvalidCastException || e is OverflowException)
            {
                throw new InvalidCastException($"The data point's {coordinateName} value - '{coordinateValue}', is not of a numeric type", e);
            }
        }

        /// <summary>
        /// Converts a point's coordinate value to datetime.
        /// </summary>
        /// <param name="coordinateValue">The chart alert property that should be displayed.</param>
        /// <param name="coordinateName">The coordinate name (e.g. X, Y).</param>
        /// <returns>The coordinate value as datetime</returns>
        private static DateTime ConvertCoordinateValueToDateTime(object coordinateValue, string coordinateName)
        {
            try
            {
                return Convert.ToDateTime(coordinateValue, CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is InvalidCastException)
            {
                throw new InvalidCastException($"The data point's {coordinateName} value - '{coordinateValue}', is not of a DateTime type", e);
            }
        }

        /// <summary>
        /// Initializes a chart with X axis from type <see cref="AlertPresentation.ChartAxisType.DateAxis"/>.
        /// </summary>
        /// <param name="chartAlertProperty">The chart alert property that should be displayed.</param>
        private void InitializeDateTimeXAxisChart(ChartAlertProperty chartAlertProperty)
        {
            var sortedDateTimePoints = chartAlertProperty.DataPoints.OrderBy(point => point.X).ToList();

            var chartDatePoints = sortedDateTimePoints.Select(dataPoint =>
            {
                DateTime x = ConvertCoordinateValueToDateTime(dataPoint.X, "X");
                double y = ConvertCoordinateValueToDouble(dataPoint.Y, "Y");

                return new ChartDataPoint<DateTime>(x, y);
            });

            var chartValues = new ChartValues<ChartDataPoint<DateTime>>(chartDatePoints);

             // In order to support Bar Chart, Since we are using DateTime.Ticks as X, the width of the bar is 1 tick and 1 tick is 1 millisecond.
             // In order to make our bars visible we need to change the unit of the chart. Hence, we are taking the difference between 2 first points (assuming difference between every 2 points is equal)
            double xAxisFactor = chartValues.Count > 1 ?
                chartValues.Skip(1).First().X.Ticks - chartValues.First().X.Ticks :
                chartValues.First().X.Ticks;

            CartesianMapper<ChartDataPoint<DateTime>> pointMapperConfig = Mappers.Xy<ChartDataPoint<DateTime>>()
                .X(dateTimeDataPoint => dateTimeDataPoint.X.Ticks / xAxisFactor)
                .Y(dateTimeDataPoint => dateTimeDataPoint.Y);

            this.SeriesCollection = new SeriesCollection(pointMapperConfig);

            this.AddSeries(chartAlertProperty.ChartType, chartValues);

            this.XAxisFormatter = value => new DateTime((long)(value * xAxisFactor)).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initializes a chart with X axis from type <see cref="AlertPresentation.ChartAxisType.NumberAxis"/>.
        /// </summary>
        /// <param name="chartAlertProperty">The chart alert property that should be displayed.</param>
        private void InitializeNumberXAxisChart(ChartAlertProperty chartAlertProperty)
        {
            var sortedNumericPoints = chartAlertProperty.DataPoints.OrderBy(point => point.X).ToList();

            var chartDatePoints = sortedNumericPoints.Select(dataPoint =>
            {
                double x = ConvertCoordinateValueToDouble(dataPoint.X, "X");
                double y = ConvertCoordinateValueToDouble(dataPoint.Y, "Y");

                return new ChartDataPoint<double>(x, y);
            });

            var chartValues = new ChartValues<ChartDataPoint<double>>(chartDatePoints);

            CartesianMapper<ChartDataPoint<double>> pointMapperConfig = Mappers.Xy<ChartDataPoint<double>>()
                .X(dateTimeDataPoint => dateTimeDataPoint.X)
                .Y(dateTimeDataPoint => dateTimeDataPoint.Y);

            this.SeriesCollection = new SeriesCollection(pointMapperConfig);

            this.AddSeries(chartAlertProperty.ChartType, chartValues);

            this.XAxisFormatter = value => value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds a new series to the chart with the given <paramref name="chartValues"/>.
        /// </summary>
        /// <typeparam name="T">The X value of a chart data point.</typeparam>
        /// <param name="chartType">The chart type.</param>
        /// <param name="chartValues">The chart values.</param>
        private void AddSeries<T>(ChartType chartType, ChartValues<ChartDataPoint<T>> chartValues)
        {
            if (chartType == ChartType.LineChart)
            {
                this.SeriesCollection.Add(new LineSeries
                {
                    Values = chartValues,
                    Stroke = SeriesColor,
                    Fill = Brushes.Transparent
                });
            }
            else
            {
                this.SeriesCollection.Add(new ColumnSeries
                {
                    Values = chartValues,
                    Stroke = SeriesColor,
                    Fill = SeriesColor
                });
            }
        }
    }
}