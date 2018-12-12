//-----------------------------------------------------------------------
// <copyright file="ChartPropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartPoint;

    /// <summary>
    /// The view model class for the <see cref="ChartPropertyControl"/> control.
    /// </summary>
    public class ChartPropertyControlViewModel : ObservableObject
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;
        private static readonly Brush BaselineColor = Brushes.LightGray;
        private static readonly Brush AnomalyColor = new SolidColorBrush(Color.FromRgb(238, 83, 80));

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
        /// Initializes a new instance of the <see cref="ChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="chartAlertPropertiesContainer">The chart alert properties container that should be displayed.</param>
        public ChartPropertyControlViewModel(ChartAlertPropertiesContainer chartAlertPropertiesContainer)
        {
            this.Title = chartAlertPropertiesContainer.ChartsAlertProperties
                .First(prop => prop.DisplayName.EndsWith("_Value", StringComparison.InvariantCulture)).DisplayName;

            this.Title = this.Title.Replace("_Value", string.Empty);

            List<ChartPoint> anomaliesChart = chartAlertPropertiesContainer.ChartsAlertProperties
                .First(prop => prop.DisplayName.EndsWith("_Anomalies", StringComparison.InvariantCulture)).DataPoints;

            var anomaliesChartXValues = new HashSet<DateTime>();
            foreach (var anomalyPoint in anomaliesChart)
            {
                try
                {
                    anomaliesChartXValues.Add(Convert.ToDateTime(anomalyPoint.X, CultureInfo.InvariantCulture));
                }
                catch (Exception e) when (e is FormatException || e is InvalidCastException)
                {
                    throw new InvalidCastException($"The data point's Y value '{anomalyPoint.X}' is not of DateTime type", e);
                }
            }

            Func<ChartDataPoint<DateTime>, object> anomalyColorPredicate = dateTimeDataPoint =>
            {
                return anomaliesChartXValues.Contains(dateTimeDataPoint.X) ? AnomalyColor : null;
            };

            CartesianMapper<ChartDataPoint<DateTime>> pointMapperConfig = Mappers.Xy<ChartDataPoint<DateTime>>()
                .X(dateTimeDataPoint => (double)dateTimeDataPoint.X.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dateTimeDataPoint => dateTimeDataPoint.Y)
                .Fill(anomalyColorPredicate)
                .Stroke(anomalyColorPredicate);

            this.SeriesCollection = new SeriesCollection(pointMapperConfig);

            this.XAxisFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString(CultureInfo.InvariantCulture);

            ChartValues<ChartDataPoint<DateTime>> lowValues = null, highValues = null, values = null;
            foreach (var chartAlertProperty in chartAlertPropertiesContainer.ChartsAlertProperties)
            {
                var chartValues = new ChartValues<ChartDataPoint<DateTime>>();

                foreach (var dataPoint in chartAlertProperty.DataPoints)
                {
                    DateTime dateTime;
                    try
                    {
                        dateTime = Convert.ToDateTime(dataPoint.X, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e) when (e is FormatException || e is InvalidCastException)
                    {
                        throw new InvalidCastException($"The data point's Y value '{dataPoint.X}' is not of DateTime type", e);
                    }

                    double value;
                    try
                    {
                        value = Convert.ToDouble(dataPoint.Y, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e) when (e is FormatException || e is InvalidCastException || e is OverflowException)
                    {
                        throw new InvalidCastException($"The data point's X value '{dataPoint.X}' is not of a numeric type", e);
                    }

                    chartValues.Add(new ChartDataPoint<DateTime>(dateTime, value));
                }

                if (chartAlertProperty.DisplayName.EndsWith("Value", StringComparison.InvariantCulture))
                {
                    values = chartValues;
                }
                else if (chartAlertProperty.DisplayName.EndsWith("High", StringComparison.InvariantCulture))
                {
                    highValues = chartValues;
                }
                else if (chartAlertProperty.DisplayName.EndsWith("Low", StringComparison.InvariantCulture))
                {
                    lowValues = chartValues;
                }
            }

            if (lowValues != null)
            {
                this.SeriesCollection.Add(new LineSeries()
                {
                    Title = "Low",
                    Values = lowValues,
                    Stroke = BaselineColor,
                    PointGeometrySize = 0,
                    Fill = Brushes.Transparent,
                    LabelPoint = this.LabelPoint
                });
            }

            if (highValues != null)
            {
                this.SeriesCollection.Add(new LineSeries
                {
                    Title = "High",
                    Values = highValues,
                    Stroke = BaselineColor,
                    PointGeometrySize = 0,
                    Fill = Brushes.Transparent,
                    LabelPoint = this.LabelPoint
                });
            }

            if (values != null)
            {
                this.SeriesCollection.Add(new LineSeries()
                {
                    Title = "Value",
                    Values = values,
                    Stroke = SeriesColor,
                    PointGeometrySize = 4,
                    Fill = Brushes.Transparent,
                    LabelPoint = this.LabelPoint
                });
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
        /// Initializes a chart with X axis from type <see cref="SmartDetectors.ChartAxisType.DateAxis"/>.
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
        /// Initializes a chart with X axis from type <see cref="SmartDetectors.ChartAxisType.NumberAxis"/>.
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
                    Fill = Brushes.Transparent,
                    LabelPoint = this.LabelPoint
                });
            }
            else
            {
                this.SeriesCollection.Add(new ColumnSeries
                {
                    Values = chartValues,
                    Stroke = SeriesColor,
                    Fill = SeriesColor,
                    LabelPoint = this.LabelPoint
                });
            }
        }

        /// <summary>
        /// Returns the point label - with thousands separator and up to 3 decimal digits
        /// </summary>
        /// <param name="p">The point</param>
        /// <returns>The label</returns>
        private string LabelPoint(LiveCharts.ChartPoint p)
        {
            return $"{p.Y:#,##0.###}";
        }
    }
}