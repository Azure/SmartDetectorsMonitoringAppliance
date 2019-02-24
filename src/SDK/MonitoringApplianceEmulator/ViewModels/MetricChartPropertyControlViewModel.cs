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
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Wpf;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Unity;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartPoint;

    /// <summary>
    /// The view model class for the <see cref="ChartPropertyControl"/> control.
    /// </summary>
    public class MetricChartPropertyControlViewModel : ObservableObject
    {
        private SeriesCollection seriesCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChartPropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="metricChartAlertProperty">The chart alert properties container that should be displayed.</param>
        public MetricChartPropertyControlViewModel(MetricChartAlertProperty metricChartAlertProperty)
        {
            this.Title = metricChartAlertProperty.DisplayName;

            // TODO: Implement charts
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
    }
}