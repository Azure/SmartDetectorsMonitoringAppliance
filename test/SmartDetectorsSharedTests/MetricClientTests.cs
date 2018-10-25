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
            IMetricClient metricClient = new MetricClient(this.tracerMock.Object, this.resourceIdentifier.SubscriptionId, this.monitorManagementClientMock.Object);

            List<MetricQueryResult> metrics = (await metricClient.GetResourceMetricsAsync(this.resourceIdentifier, ServiceType.AzureStorageQueue, new QueryParameters(), default(CancellationToken))).ToList();

            // Validate that right Uri was generated
            this.metricsOperationsMock.Verify(metric => metric.ListWithHttpMessagesAsync(expectedUri, It.IsAny<ODataQuery<MetadataValueInner>>(), null, null, string.Empty, null, null, null, null, null, null, CancellationToken.None));
            Assert.AreEqual(metrics.Count, 2, "2 metrics are expected");

            // Validate first metric was converted successfully
            Assert.AreEqual("MetricName1", metrics[0].Name, "First metric name isn't correct");
            Assert.AreEqual("ByteSeconds", metrics[0].Unit, "First metric unit isn't correct");
            Assert.AreEqual(0, metrics[0].Timeseries.Count, "First metric timeseries should be empty");

            // Validate second metric was converted successfully
            Assert.AreEqual("MetricName2", metrics[1].Name, "Second metric name isn't correct");
            Assert.AreEqual("MilliSeconds", metrics[1].Unit, "Second metric unit isn't correct");
            Assert.AreEqual(5, metrics[1].Timeseries.Count, "Second metric timeseries should be empty");
            Assert.AreEqual(2, metrics[1].Timeseries[0].Data.Count, "Second metric first timeseries (Dimension1Value1) length should be 2");
            Assert.AreEqual(2, metrics[1].Timeseries[1].Data.Count, "Second metric second timeseries (Dimension1Value2) length should be 2");
            Assert.AreEqual(1.1, metrics[1].Timeseries[0].Data[0].Average, "Second metric first timeseries first average is wrong");
            Assert.AreEqual(1.0, metrics[1].Timeseries[0].Data[1].Average, "Second metric first timeseries second average is wrong");
            Assert.AreEqual(timestamp.AddMinutes(-1), metrics[1].Timeseries[0].Data[0].TimeStamp, "Second metric first timeseries first timestamp is wrong");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Total, "Second metric first timeseries first total should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Maximum, "Second metric first timeseries first maximum should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Minimum, "Second metric first timeseries first minimum should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Count, "Second metric first timeseries first count should be null");

            Assert.AreEqual(2.1, metrics[1].Timeseries[1].Data[0].Minimum, "Second metric second timeseries first minimum is wrong");
            Assert.AreEqual(3.1, metrics[1].Timeseries[2].Data[0].Maximum, "Second metric third timeseries first maximum is wrong");
            Assert.AreEqual(4.1, metrics[1].Timeseries[3].Data[0].Total, "Second metric forth timeseries first total is wrong");
            Assert.AreEqual(1, metrics[1].Timeseries[4].Data[0].Count, "Second metric fifth timeseries first count is wrong");

            Assert.AreEqual(1, metrics[1].Timeseries[0].MetaData.Count, "Second metric first timeseries metadata length is wrong");
            Assert.AreEqual("Dimension1", metrics[1].Timeseries[0].MetaData[0].Key, "Second metric first timeseries first metadata key is wrong");
            Assert.AreEqual("Dimension1Value1", metrics[1].Timeseries[0].MetaData[0].Value, "Second metric first timeseries first metadata value is wrong");
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
            IMetricClient metricClient = new MetricClient(this.tracerMock.Object, this.resourceIdentifier.SubscriptionId, this.monitorManagementClientMock.Object);

            List<MetricDefinition> definitions = (await metricClient.GetResourceMetricDefinitionsAsync(resourceUri, default(CancellationToken))).ToList();

            // Validate that right Uri was used
            this.metricDefinitionsOperationsMock.Verify(metric => metric.ListWithHttpMessagesAsync(resourceUri, null, null, CancellationToken.None));
            Assert.AreEqual(definitions.Count, 2, "2 definitions are expected");

            // Validate first metric was converted successfully
            Assert.AreEqual(expectedDefinitions.Count, definitions.Count, "Definition count isn't correct");
            for (int i = 0; i < expectedDefinitions.Count; i++)
            {
                Assert.AreEqual(expectedDefinitions[i].Name.Value, definitions[i].Name, "Metric name isn't correct");
                Assert.IsTrue(expectedDefinitions[i].MetricAvailabilities.Select(x => Tuple.Create(x.Retention, x.TimeGrain)).SequenceEqual(definitions[i].Availabilities), "Metric availabilities aren't correct");
                Assert.IsTrue(expectedDefinitions[i].Dimensions.Select(x => x.Value).SequenceEqual(definitions[i].Dimensions), "Metric dimensions aren't correct");
                Assert.AreEqual(expectedDefinitions[i].IsDimensionRequired, definitions[i].IsDimensionRequired, "Metric is dimension required isn't correct");
                Assert.AreEqual(expectedDefinitions[i].PrimaryAggregationType?.ToString(), definitions[i].PrimaryAggregationType?.ToString(), "Metric primary aggregation type isn't correct");
                Assert.AreEqual(expectedDefinitions[i].Unit?.ToString(), definitions[i].Unit, "Metric unit isn't correct");
            }
        }

        [TestMethod]
        public void AllServiceTypesExistInMappingDictionary()
        {
            foreach (ServiceType serviceType in Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Where(t => t != ServiceType.None))
            {
                Assert.IsTrue(MetricClient.MapAzureServiceTypeToPresentationInUri.Keys.Contains(serviceType), $"Service {serviceType} is missing in service mapping dictionary");
            }
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
            MetricClient client = new MetricClient(this.tracerMock.Object, credentialsFactory, this.resourceIdentifier.SubscriptionId);

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
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value1") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), average: 1.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), average: 1.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value2") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), minimum: 2.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), minimum: 2.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value3") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), maximum: 3.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), maximum: 3.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value4") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), total: 4.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), total: 4.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value5") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), count: 1) },
                                { new MetricValue(timestamp.AddMinutes(-2), count: 5) },
                            }
                        },
                    }
                }
            };
        }

        private static List<Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition> GetMetricDefinitionsList()
        {
            return new List<Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition>()
            {
                new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition(
                    isDimensionRequired: true,
                    resourceId: "resourceId",
                    name: new LocalizableString("StorageSize"),
                    unit: Unit.Bytes,
                    primaryAggregationType: AggregationType.Maximum,
                    metricAvailabilities: new List<MetricAvailability>()
                    {
                        new MetricAvailability(TimeSpan.FromMinutes(60), TimeSpan.FromDays(30)),
                        new MetricAvailability(TimeSpan.FromMinutes(1), null),
                    },
                    id: "id1",
                    dimensions: new List<LocalizableString>()
                    {
                        new LocalizableString("dim1"),
                        new LocalizableString("dim2"),
                        new LocalizableString("dim3"),
                    }),
                new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition(
                    isDimensionRequired: null,
                    resourceId: "resourceId",
                    name: new LocalizableString("StorageLatency"),
                    unit: Unit.MilliSeconds,
                    primaryAggregationType: null,
                    metricAvailabilities: new List<MetricAvailability>()
                    {
                        new MetricAvailability(TimeSpan.FromMinutes(30), TimeSpan.FromDays(20))
                    },
                    id: "id2",
                    dimensions: new List<LocalizableString>())
            };
        }
    }
}
