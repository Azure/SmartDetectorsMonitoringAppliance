//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactoryTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AnalysisServicesFactoryTests
    {
        private const int TooManyResourcesCount = 11;
        private const string SubscriptionId = "subscriptionId";
        private const string ResourceGroupName = "resourceGroupName";
        private const string ResourceName = "resourceName";
        private const string WorkSpaceName = "workspaceName";

        private Mock<IExtendedTracer> tracerMock;
        private Mock<ICredentialsFactory> credentialsFactoryMock;
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<IExtendedTracer>();
            this.credentialsFactoryMock = new Mock<ICredentialsFactory>();
            this.credentialsFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(() => new EmptyCredentials());
            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            this.azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();

            Environment.SetEnvironmentVariable("APPSETTING_AnalyticsQueryTimeoutInMinutes", "15");
        }

        [TestMethod]
        public async Task WhenCreatingApplicationInsightsClientThenTheCorrectClientIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            TelemetryDataClientBase client = await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken)) as TelemetryDataClientBase;

            Assert.IsNotNull(client);
            Assert.AreEqual(typeof(ApplicationInsightsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            CollectionAssert.AreEqual(new[] { resources.First().ToResourceId() }, client.TelemetryResourceIds.ToArray(), "Wrong resource Ids");
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientThenTheCorrectClientIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            TelemetryDataClientBase client = await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken)) as TelemetryDataClientBase;

            Assert.IsNotNull(client);
            Assert.AreEqual(typeof(LogAnalyticsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            CollectionAssert.AreEqual(new[] { resources.First().ToResourceId() }, client.TelemetryResourceIds.ToArray(), "Wrong resource Ids");
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientOnlyWithResourcesOfTypeLogAnalyticsThenTheCorrectClientIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            TelemetryDataClientBase client = await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken)) as TelemetryDataClientBase;

            Assert.IsNotNull(client);
            Assert.AreEqual(typeof(LogAnalyticsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            CollectionAssert.AreEqual(new[] { resources.First().ToResourceId() }, client.TelemetryResourceIds.ToArray(), "Wrong resource Ids");
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingApplicationInsightsClientWithWrongTypeThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientWithWrongTypeThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientWithEmptyResourcesListThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>();

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingApplicationInsightsClientWithEmptyResourcesListThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>();

            this.SetupTest(resources);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingApplicationInsightsClientWithMixedResourcesThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName + "111"),
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientWithMixedResourcesOfLogAnalyticsAndApplicationInsightsThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkSpaceName),
                new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientWithoutLogAnalyticsResourcesThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.Is<string>(y => string.Equals(y, "subscriptionId", StringComparison.InvariantCulture)), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ResourceIdentifier>());

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientWithTooManyResourcesThenAnExceptionIsThrown()
        {
            List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                .Select(i => new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingApplicationInsightsClientWithTooManyResourcesThenAnExceptionIsThrown()
        {
            List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                .Select(i => new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenCreatingMetricClientThenItIsCreatedSuccessfully()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            IMetricClient client = await factory.CreateMetricClientAsync(SubscriptionId, default(CancellationToken));
            Assert.IsNotNull(client);
            Assert.IsTrue(client is MetricClient);
        }

        [TestMethod]
        public async Task WhenCreatingActivityLogClientThenItIsCreatedSuccessfully()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            IActivityLogClient client = await factory.CreateActivityLogClientAsync(default(CancellationToken));
            Assert.IsNotNull(client);
            Assert.IsTrue(client is ActivityLogClient);
        }

        [TestMethod]
        public async Task WhenCreatingArmClientThenItIsCreatedSuccessfully()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            IAzureResourceManagerClient client = await factory.CreateArmClientAsync(default(CancellationToken));
            Assert.IsNotNull(client);
            Assert.IsTrue(client == this.azureResourceManagerClientMock.Object);
        }

        private void SetupTest(List<ResourceIdentifier> resources)
        {
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.Is<string>(y => string.Equals(y, "subscriptionId", StringComparison.InvariantCulture)), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resources);
        }
    }
}