//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using LiveCharts;
    using LiveCharts.Defaults;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MetricChartPropertyControlViewModelTests
    {
        private MetricChartAlertProperty property;
        private List<double> values;
        private List<DateTime> timestamps;
        private ResourceIdentifier resource;
        private Mock<ITracer> tracerMock;
        private Mock<IMetricClient> metricClientMock;
        private Mock<IAnalysisServicesFactory> analysisServicesFactoryMock;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create property
            this.resource = new ResourceIdentifier(ResourceType.AzureStorage, "subscriptionId", "resourceGroupName", "storage1");
            this.property = new MetricChartAlertProperty(
                propertyName: "metric",
                displayName: "metric display",
                order: 1,
                metricName: "metric1",
                timeGrain: TimeSpan.FromMinutes(5),
                aggregationType: AggregationType.Sum)
                {
                    StartTimeUtc = new DateTime(2019, 4, 1, 10, 0, 0),
                    EndTimeUtc = new DateTime(2019, 4, 1, 12, 0, 0),
                    MetricNamespace = "nameSpace",
                    ResourceId = this.resource.ToResourceId()
                };

            // Create metric data
            this.values = Enumerable.Range(0, (int)((this.property.EndTimeUtc - this.property.StartTimeUtc).Value.Ticks / this.property.TimeGrain.Ticks)).Select(n => (double)n).ToList();
            this.timestamps = this.values.Select(n => new DateTime((long)(this.property.StartTimeUtc.Value.Ticks + (this.property.TimeGrain.Ticks * n)))).ToList();
            MetricQueryResult metricQueryResult = new MetricQueryResult("name", "unit", new List<MetricTimeSeries>()
            {
                new MetricTimeSeries(
                    this.timestamps.Zip(this.values, (t, v) => new MetricValues(t, v, v, v, v, 1)).ToList(),
                    new List<KeyValuePair<string, string>>() { })
            });

            // Mock metric client result
            this.metricClientMock = new Mock<IMetricClient>();
            this.metricClientMock
                .Setup(x => x.GetResourceMetricsAsync(this.resource.ToResourceId(), It.IsAny<QueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string resourceId, QueryParameters queryParameters, CancellationToken ct) =>
                {
                    // Verify the query parameters
                    Assert.AreEqual(Aggregation.Total, queryParameters.Aggregations.Single());
                    Assert.AreEqual(TimeSpan.FromMinutes(5), queryParameters.Interval);
                    Assert.AreEqual(new DateTime(2019, 4, 1, 10, 0, 0), queryParameters.StartTime);
                    Assert.AreEqual(new DateTime(2019, 4, 1, 12, 0, 0), queryParameters.EndTime);
                    Assert.AreEqual("metric1", queryParameters.MetricNames.Single());
                    Assert.AreEqual("nameSpace", queryParameters.MetricNamespace);

                    return new[] { metricQueryResult };
                });

            this.analysisServicesFactoryMock = new Mock<IAnalysisServicesFactory>();
            this.analysisServicesFactoryMock
                .Setup(x => x.CreateMetricClientAsync(this.resource.SubscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(this.metricClientMock.Object);

            this.tracerMock = new Mock<ITracer>();
        }

        [TestMethod]
        public async Task WhenCreatingViewModelForMetricChartWithDataThenThenItIsCreatedSuccessfully()
        {
            // Create view model
            MetricChartPropertyControlViewModel viewModel = await this.CreateViewModel();

            // Verify results
            Assert.IsFalse(viewModel.ReadChartValuesTask.IsRunning);
            ChartValues<DateTimePoint> points = viewModel.ReadChartValuesTask.Result;
            Assert.IsNotNull(points);
            Enumerable.SequenceEqual(this.timestamps, points.Select(p => p.DateTime));
            Enumerable.SequenceEqual(this.values, points.Select(p => p.Value));
        }

        [TestMethod]
        public async Task WhenCreatingViewModelForMetricChartWithInvalidStartTimeThenAnExceptionIsThrown()
        {
            this.property.StartTimeUtc = null;
            MetricChartPropertyControlViewModel viewModel = await this.CreateViewModel();
            Assert.IsNull(viewModel.ReadChartValuesTask.Result);
        }

        [TestMethod]
        public async Task WhenCreatingViewModelForMetricChartWithInvalidEndTimeThenAnExceptionIsThrown()
        {
            this.property.EndTimeUtc = null;
            MetricChartPropertyControlViewModel viewModel = await this.CreateViewModel();
            Assert.IsNull(viewModel.ReadChartValuesTask.Result);
        }

        private async Task<MetricChartPropertyControlViewModel> CreateViewModel()
        {
            // Create view model
            MetricChartPropertyControlViewModel viewModel = new MetricChartPropertyControlViewModel(
                this.property,
                this.analysisServicesFactoryMock.Object,
                this.tracerMock.Object);

            // Wait till the view is ready
            int count = 0;
            while (viewModel.ReadChartValuesTask.IsRunning && count < 100)
            {
                count++;
                await Task.Delay(100);
            }

            return viewModel;
        }
    }
}
