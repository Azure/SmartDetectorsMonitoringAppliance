//-----------------------------------------------------------------------
// <copyright file="ChartPropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Globalization;
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models.Chart;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// The view model class for the <see cref="ChartPropertyControlViewModel"/> control.
    /// </summary>
    public class ChartPropertyControlViewModel : ObservableObject
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;

        private SeriesCollection seriesCollection;

        private Func<double, string> xAxisFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="chartAlertProperty">The table alert property that should be displayed.</param>
        public ChartPropertyControlViewModel(ChartAlertProperty chartAlertProperty)
        {
            var chartValues = new ChartValues<DateTimeDataPoint>();

            foreach (var dataPoint in chartAlertProperty.DataPoints)
            {
                DateTime dateTime;
                try
                {
                    dateTime = Convert.ToDateTime(dataPoint.X, CultureInfo.InvariantCulture);
                }
                catch (Exception e) when (e is FormatException || e is InvalidCastException)
                {
                    throw new InvalidCastException($"The data point Y value '{dataPoint.X}' is not of DateTime type", e);
                }

                double value;
                try
                {
                    value = Convert.ToDouble(dataPoint.Y, CultureInfo.InvariantCulture);
                }
                catch (Exception e) when (e is FormatException || e is InvalidCastException || e is OverflowException)
                {
                    throw new InvalidCastException($"The data point X value '{dataPoint.X}' is not of a numeric type", e);
                }

                chartValues.Add(new DateTimeDataPoint(dateTime, value));
            }

            /*
             * Relevant for Column Series:
             * Since we are using DateTime.Ticks as X, the width of the bar is 1 tick and 1 tick is 1 millisecond.
             * In order to make our bars visible we need to change the unit of the chart. For the initial view are going to use hours.
             * There is a future task to create this scale dynamically according to the X values range size.
             */
            CartesianMapper<DateTimeDataPoint> pointMapperConfig = Mappers.Xy<DateTimeDataPoint>()
                .X(dateTimeDataPoint => (double)dateTimeDataPoint.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dateTimeDataPoint => dateTimeDataPoint.Value);

            this.SeriesCollection = new SeriesCollection(pointMapperConfig);

            if (chartAlertProperty.ChartType == ChartType.LineChart)
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

            this.XAxisFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString(CultureInfo.InvariantCulture);
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
        /// Gets the series collection.
        /// </summary>
        public Func<double, string> XAxisFormatter
        {
            get
            {
                return this.xAxisFormatter;
            }

            private set
            {
                this.xAxisFormatter = value;
                this.OnPropertyChanged();
            }
        }
    }
}