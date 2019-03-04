//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Defaults;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// The view model class for the <see cref="MetricChartPropertyControl"/> control.
    /// </summary>
    public class MetricChartPropertyControlViewModel : ObservableObject
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;
        private static readonly Brush BaselineColor = Brushes.LightGray;
        private static readonly Brush AnomalyColor = new SolidColorBrush(Color.FromRgb(238, 83, 80));

        private readonly Task loadChartTask;
        private readonly IInternalAnalysisServicesFactory analysisServicesFactory;
        private SeriesCollection seriesCollection;
        private Func<double, string> xAxisFormatter;
        private Func<double, string> yAxisFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="analysisServicesFactory">The analysis services factory</param>
        /// <param name="metricChartAlertProperty">The metric chart alert property that should be displayed.</param>
        public MetricChartPropertyControlViewModel(IInternalAnalysisServicesFactory analysisServicesFactory, MetricChartAlertProperty metricChartAlertProperty)
        {
            this.Title = metricChartAlertProperty.DisplayName;
            this.analysisServicesFactory = analysisServicesFactory;
            this.loadChartTask = this.LoadChart(metricChartAlertProperty, CancellationToken.None);
        }

        /// <summary>
        /// Gets the chart title.
        /// </summary>
        public string Title { get; }

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
        /// Gets the X axis formatter.
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

        /// <summary>
        /// Gets the Y axis formatter.
        /// </summary>
        public Func<double, string> YAxisFormatter
        {
            get
            {
                return this.yAxisFormatter;
            }

            private set
            {
                this.yAxisFormatter = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the chart is loaded
        /// </summary>
        public TaskStatus IsLoaded => this.loadChartTask?.Status ?? TaskStatus.WaitingToRun;

        private async Task LoadChart(MetricChartAlertProperty property, CancellationToken cancellationToken)
        {
            // Convert from aggregation type to aggregation
            Aggregation aggregation;
            switch (property.AggregationType)
            {
                case AggregationType.Average:
                    aggregation = Aggregation.Average;
                    break;
                case AggregationType.Count:
                    aggregation = Aggregation.Count;
                    break;
                case AggregationType.Sum:
                    aggregation = Aggregation.Total;
                    break;
                default:
                    throw new ApplicationException($"Invalid aggregation type {property.AggregationType}");
            }

            // Get the resource
            ResourceIdentifier resource = ResourceIdentifier.CreateFromResourceId(property.ResourceId);

            // Create the metrics client
            IMetricClient metricClient = await this.analysisServicesFactory.CreateMetricClientAsync(resource.SubscriptionId, cancellationToken)
                .ConfigureAwait(false);

            // Send a metric query using the metric client
            IEnumerable<MetricQueryResult> metricQueryResults = await metricClient.GetResourceMetricsAsync(
                    resource,
                    ServiceType.None,
                    new QueryParameters()
                    {
                        MetricNamespace = property.MetricNamespace,
                        MetricNames = new List<string>() { property.MetricName },
                        StartTime = property.StartTimeUtc,
                        EndTime = property.EndTimeUtc,
                        Interval = property.TimeGrain,
                        Aggregations = new List<Aggregation>() { aggregation },
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            // Get chart points
            ChartValues<DateTimePoint> values = new ChartValues<DateTimePoint>(
                metricQueryResults
                    .Single()
                    .Timeseries
                    .Single()
                    .Data
                    .Select(p => new DateTimePoint(p.TimeStamp, p.GetValue(aggregation))));

            // Get low/high thresholds (mock)
            double percentile10 = values.OrderBy(p => p.Value).ElementAt((int)Math.Floor(values.Count * 0.1)).Value;
            ChartValues<DateTimePoint> low = new ChartValues<DateTimePoint>(values.Select(p => new DateTimePoint(p.DateTime, percentile10)));
            double percentile90 = values.OrderBy(p => p.Value).ElementAt((int)Math.Floor(values.Count * 0.9)).Value;
            ChartValues<DateTimePoint> high = new ChartValues<DateTimePoint>(values.Select(p => new DateTimePoint(p.DateTime, percentile90)));

            // Predicate that indicates whether a point is an anomaly
            var dateTimeToThresholds = low.Zip(high, (p1, p2) => new
                {
                    p1.DateTime, Low = p1.Value, High = p2.Value
                })
                .ToDictionary(x => x.DateTime, x => x);
            Func<DateTimePoint, object> anomalyColorPredicate = p =>
            {
                if (dateTimeToThresholds.TryGetValue(p.DateTime, out var t))
                {
                    if (p.Value < t.Low || p.Value > t.High)
                    {
                        return AnomalyColor;
                    }
                }

                return null;
            };

            // Update the view model properties (on the UI thread)
            Application.Current.Dispatcher.Invoke(() =>
            {
                CartesianMapper<DateTimePoint> pointMapperConfig = Mappers.Xy<DateTimePoint>()
                    .X(dateTimeDataPoint => dateTimeDataPoint.DateTime.Ticks * 1.0 / TimeSpan.FromHours(1).Ticks)
                    .Y(dateTimeDataPoint => dateTimeDataPoint.Value)
                    .Fill(anomalyColorPredicate)
                    .Stroke(anomalyColorPredicate);

                this.SeriesCollection = new SeriesCollection(pointMapperConfig)
                {
                    new LineSeries()
                    {
                        Title = "Low",
                        Values = low,
                        Stroke = BaselineColor,
                        PointGeometrySize = 0,
                        Fill = Brushes.Transparent,
                        LabelPoint = this.LabelPoint
                    },
                    new LineSeries
                    {
                        Title = "High",
                        Values = high,
                        Stroke = BaselineColor,
                        PointGeometrySize = 0,
                        Fill = Brushes.Transparent,
                        LabelPoint = this.LabelPoint
                    },
                    new LineSeries()
                    {
                        Title = "Value",
                        Values = values,
                        Stroke = SeriesColor,
                        PointGeometrySize = 4,
                        Fill = Brushes.Transparent,
                        LabelPoint = this.LabelPoint
                    }
                };

                this.XAxisFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString(CultureInfo.InvariantCulture);
                this.YAxisFormatter = value => value.ToString(CultureInfo.InvariantCulture);
            });
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