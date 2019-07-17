//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// The view model class for the <see cref="MetricChartPropertyControl"/> control.
    /// </summary>
    public class MetricChartPropertyControlViewModel : ObservableObject
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;

        private readonly ObservableTask<ChartValues<DateTimePoint>> loadChartTask;
        private readonly IAnalysisServicesFactory analysisServicesFactory;
        private readonly ITracer tracer;
        private SeriesCollection seriesCollection;
        private Func<double, string> xAxisFormatter;
        private Func<double, string> yAxisFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="analysisServicesFactory">The analysis services factory</param>
        /// <param name="metricChartAlertProperty">The metric chart alert property that should be displayed.</param>
        /// <param name="tracer">The tracer</param>
        public MetricChartPropertyControlViewModel(
            MetricChartAlertProperty metricChartAlertProperty,
            IAnalysisServicesFactory analysisServicesFactory,
            ITracer tracer)
        {
            this.Title = metricChartAlertProperty.DisplayName;
            this.analysisServicesFactory = analysisServicesFactory;
            this.tracer = tracer;

            // Start a task to read the metric values
            this.loadChartTask = new ObservableTask<ChartValues<DateTimePoint>>(
                this.ReadChartValues(metricChartAlertProperty),
                tracer,
                this.DisplayChartValues);
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
        /// Asynchronously load the metric chart
        /// </summary>
        /// <param name="property">The metric chart alert property</param>
        /// <returns>A <see cref="Task"/>, running the current operation</returns>
        private async Task<ChartValues<DateTimePoint>> ReadChartValues(MetricChartAlertProperty property)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            // Verify start/end times
            DateTime startTime = property.StartTimeUtc ?? throw new ApplicationException("Start time cannot be null");
            DateTime endTime = property.EndTimeUtc ?? throw new ApplicationException("End time cannot be null");

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
                    StorageServiceType.None,
                    new QueryParameters()
                    {
                        MetricNamespace = property.MetricNamespace,
                        MetricNames = new List<string>() { property.MetricName },
                        StartTime = startTime,
                        EndTime = endTime,
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

            return values;
        }

        private void DisplayChartValues(ChartValues<DateTimePoint> values)
        {
            if (values == null)
            {
                MessageBox.Show($"Error loading chart {this.Title}");
                return;
            }

            // Set point mapper to map the DateTimePoint to X/Y values
            CartesianMapper<DateTimePoint> pointMapperConfig = Mappers.Xy<DateTimePoint>()
                .X(dateTimeDataPoint => dateTimeDataPoint.DateTime.Ticks * 1.0 / TimeSpan.FromHours(1).Ticks)
                .Y(dateTimeDataPoint => dateTimeDataPoint.Value);

            // Create a series for the metric values
            this.SeriesCollection = new SeriesCollection(pointMapperConfig)
            {
                    new LineSeries()
                    {
                        Values = values,
                        Stroke = SeriesColor,
                        Fill = Brushes.Transparent,
                    }
            };

            // Set X/Y axis formatters
            this.XAxisFormatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString(CultureInfo.InvariantCulture);
            this.YAxisFormatter = value => value.ToString(CultureInfo.InvariantCulture);
        }
    }
}