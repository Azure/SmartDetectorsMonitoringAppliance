//-----------------------------------------------------------------------
// <copyright file="MetricClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.Rest.Azure;
    using Microsoft.Rest.Azure.OData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using MetricDefinition = Microsoft.Azure.Monitoring.SmartDetectors.Metric.MetricDefinition;

    [TestClass]
    public class MetricClientTests
    {
        private Mock<IExtendedTracer> tracerMock;
        private Mock<IMonitorManagementClient> monitorManagementClientMock;
        private Mock<IMetricsOperations> metricsOperationsMock;
        private Mock<IMetricDefinitionsOperations> metricDefinitionsOperationsMock;
        private ResourceIdentifier resourceIdentifier;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<IExtendedTracer>();
            this.monitorManagementClientMock = new Mock<IMonitorManagementClient>();
            this.metricsOperationsMock = new Mock<IMetricsOperations>();
            this.metricDefinitionsOperationsMock = new Mock<IMetricDefinitionsOperations>();
            this.resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");
        }

        [TestMethod]
        public async Task WhenCallingGetResourceMetricsWithServiceTypeHappyFlow()
        {
            var timestamp = DateTime.UtcNow;
            string expectedUri = "/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/queueServices/default";

            var azureResponse = new AzureOperationResponse<ResponseInner>()
            {
                Body = new ResponseInner("timespan", GetMetricList(timestamp)),
                Request = new HttpRequestMessage(),
                RequestId = "RequestId",
                Response = new HttpResponseMessage()
            };

            this.metricsOperationsMock
                .Setup(metric => metric.ListWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<ODataQuery<MetadataValueInner>>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<ResultType?>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(azureResponse);

            this.monitorManagementClientMock.SetupGet(monitorClient => monitorClient.Metrics).Returns(this.metricsOperationsMock.Object);
            IMetricClient metricClient = new MetricClient(this.tracerMock.Object, this.monitorManagementClientMock.Object);

            List<MetricQueryResult> metrics = (await metricClient.GetResourceMetricsAsync(this.resourceIdentifier, ServiceType.AzureStorageQueue, new QueryParameters(), default(CancellationToken))).ToList();

            // Validate that right Uri was generated
            this.metricsOperationsMock.Verify(metric => metric.ListWithHttpMessagesAsync(expectedUri, It.IsAny<ODataQuery<MetadataValueInner>>(), null, null, string.Empty, null, null, null, null, null, null, CancellationToken.None));
            Assert.AreEqual(metrics.Count, 2, "2 metrics are expected");
        }

        [TestMethod]
        public async Task WhenCallingGetResourceMetricDefinitionsHappyFlow()
        {
            string resourceUri = "/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME";

            var expectedDefinitions = GetMetricDefinitionsList();
            var azureResponse = new AzureOperationResponse<IEnumerable<Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition>>()
            {
                Body = expectedDefinitions,
                Request = new HttpRequestMessage(),
                RequestId = "RequestId",
                Response = new HttpResponseMessage()
            };

            this.metricDefinitionsOperationsMock.Setup(metric => metric.ListWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(azureResponse);

            this.monitorManagementClientMock.SetupGet(monitorClient => monitorClient.MetricDefinitions).Returns(this.metricDefinitionsOperationsMock.Object);
            IMetricClient metricClient = new MetricClient(this.tracerMock.Object, this.monitorManagementClientMock.Object);

            List<MetricDefinition> definitions = (await metricClient.GetResourceMetricDefinitionsAsync(resourceUri, default(CancellationToken))).ToList();

            // Validate that right Uri was used
            this.metricDefinitionsOperationsMock.Verify(metric => metric.ListWithHttpMessagesAsync(resourceUri, null, null, CancellationToken.None));
            Assert.AreEqual(definitions.Count, 2, "2 definitions are expected");
        }

        [Ignore]
        [TestMethod]
        public async Task WhenSendingMetricQueryThenTheResultsAreAsExpected()
        {
            // Authenticate (set real values in resourceIdentifier to run this test).
            var authenticationServices = new AuthenticationServices();
            authenticationServices.AuthenticateUser();
            ICredentialsFactory credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices);

            var resourceId = $"/subscriptions/{this.resourceIdentifier.SubscriptionId}/resourceGroups/{this.resourceIdentifier.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{this.resourceIdentifier.ResourceName}/queueServices/default";
            MetricClient client = new MetricClient(this.tracerMock.Object, credentialsFactory);

            var parameters = new QueryParameters()
            {
                StartTime = DateTime.UtcNow.Date.AddDays(-1),
                EndTime = DateTime.UtcNow.Date,
                Aggregations = new List<Aggregation> { Aggregation.Total },
                MetricNames = new List<string>() { "QueueMessageCount" },
                Interval = TimeSpan.FromMinutes(60)
            };

            var metrics1 = (await client.GetResourceMetricsAsync(resourceId, parameters)).ToList();
            var metrics2 = (await client.GetResourceMetricsAsync(this.resourceIdentifier, ServiceType.AzureStorageQueue, parameters)).ToList();
            Assert.IsTrue(metrics1.Any() && metrics2.Any(), "Lists are not full with data");
            Assert.IsTrue(metrics1.First().Timeseries.Any() && metrics2.First().Timeseries.Any(), "Metrics do not contain Time series");
            Assert.IsTrue(metrics1[0].Timeseries[0].Data.Any() && metrics2[0].Timeseries[0].Data.Any(), "Time series are not full with data");
        }

        /// <summary>
        /// Returns a synthetic metric list for test purposes
        /// </summary>
        /// <param name="timestamp">The time stamp to be used in the metric's timeseries</param>
        /// <returns>A synthetic metric list for test purposes</returns>
        private static List<Metric> GetMetricList(DateTime timestamp)
        {
            return new List<Metric>
            {
                new Metric()
                {
                    Id = "MetricId1",
                    Name = new LocalizableString("MetricName1"),
                    Unit = Unit.ByteSeconds,
                    Type = "MetricType1",
                },
                new Metric()
                {
                    Id = "MetricId2",
                    Name = new LocalizableString("MetricName2"),
                    Unit = Unit.MilliSeconds,
                    Type = "MetricType2",
                    Timeseries = new List<TimeSeriesElement>()
                    {
                        new TimeSeriesElement(),
                        new TimeSeriesElement(),
                        new TimeSeriesElement(),
                    }
                }
            };
        }

        private static List<Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition> GetMetricDefinitionsList()
        {
            return new List<Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition>()
            {
                new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition(),
                new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition()
            };
        }
    }
}
