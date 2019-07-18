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
    using LiveCharts;
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
        private readonly MetricChartAlertProperty metricChartAlertProperty;
        private readonly IAnalysisServicesFactory analysisServicesFactory;
        private readonly ITracer tracer;
        private ObservableTask<ChartValues<DateTimePoint>> readChartValuesTask;
        private Func<double, string> xAxisFormatter;
        private Func<double, string> yAxisFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="metricChartAlertProperty">The metric chart alert property that should be displayed.</param>
        /// <param name="analysisServicesFactory">The analysis services factory</param>
        /// <param name="tracer">The tracer</param>
        public MetricChartPropertyControlViewModel(
            MetricChartAlertProperty metricChartAlertProperty,
            IAnalysisServicesFactory analysisServicesFactory,
            ITracer tracer)
        {
            this.metricChartAlertProperty = metricChartAlertProperty;

            this.Title = metricChartAlertProperty.DisplayName;
            this.analysisServicesFactory = analysisServicesFactory;
            this.tracer = tracer;

            // Set X/Y axis formatters
            this.XAxisFormatter = value => (value >= 0 ? new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)) : DateTime.MinValue).ToString(CultureInfo.InvariantCulture);
            this.YAxisFormatter = value => value.ToString(CultureInfo.InvariantCulture);

            // Start a task to read the metric values
            this.ReadChartValuesTask = new ObservableTask<ChartValues<DateTimePoint>>(
                this.ReadChartValuesAsync(),
                tracer);
        }

        /// <summary>
        /// Gets the chart title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a task that loads the metric chart.
        /// </summary>
        public ObservableTask<ChartValues<DateTimePoint>> ReadChartValuesTask
        {
            get
            {
                return this.readChartValuesTask;
            }

            private set
            {
                this.readChartValuesTask = value;
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
        /// Read the metric values
        /// </summary>
        /// <returns>A <see cref="Task"/>, running the current operation, returning the metric values as a <see cref="LineSeries"/></returns>
        private async Task<ChartValues<DateTimePoint>> ReadChartValuesAsync()
        {
            CancellationToken cancellationToken = CancellationToken.None;

            // Verify start/end times
            DateTime startTime = this.metricChartAlertProperty.StartTimeUtc ?? throw new ApplicationException("Start time cannot be null");
            DateTime endTime = this.metricChartAlertProperty.EndTimeUtc ?? throw new ApplicationException("End time cannot be null");
            if (endTime > DateTime.UtcNow)
            {
                endTime = DateTime.UtcNow;
            }

            // Convert from aggregation type to aggregation
            Aggregation aggregation;
            switch (this.metricChartAlertProperty.AggregationType)
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
                case AggregationType.Maximum:
                    aggregation = Aggregation.Maximum;
                    break;
                case AggregationType.Minimum:
                    aggregation = Aggregation.Minimum;
                    break;
                default:
                    throw new ApplicationException($"Invalid aggregation type {this.metricChartAlertProperty.AggregationType}");
            }

            // Create the metrics client
            ResourceIdentifier resource = ResourceIdentifier.CreateFromResourceId(this.metricChartAlertProperty.ResourceId);
            IMetricClient metricClient = await this.analysisServicesFactory.CreateMetricClientAsync(resource.SubscriptionId, cancellationToken)
                .ConfigureAwait(false);

            // Send a metric query using the metric client
            IEnumerable<MetricQueryResult> metricQueryResults = await metricClient.GetResourceMetricsAsync(
                    this.metricChartAlertProperty.ResourceId,
                    new QueryParameters()
                    {
                        MetricNamespace = this.metricChartAlertProperty.MetricNamespace,
                        MetricNames = new List<string>() { this.metricChartAlertProperty.MetricName },
                        StartTime = startTime,
                        EndTime = endTime,
                        Interval = this.metricChartAlertProperty.TimeGrain,
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
    }
}