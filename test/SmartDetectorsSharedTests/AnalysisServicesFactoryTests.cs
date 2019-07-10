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
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AnalysisServicesFactoryTests
    {
        private const string SubscriptionId = "subscriptionId";
        private const string ResourceGroupName = "resourceGroupName";
        private const string ResourceName = "resourceName";

        private Mock<ITracer> tracerMock;
        private Mock<ICredentialsFactory> credentialsFactoryMock;
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();
            this.credentialsFactoryMock = new Mock<ICredentialsFactory>();
            this.credentialsFactoryMock.Setup(x => x.CreateServiceClientCredentials(It.IsAny<string>())).Returns(() => new EmptyCredentials());
            this.credentialsFactoryMock.Setup(x => x.CreateAzureCredentials(It.IsAny<string>())).Returns(() => new AzureCredentials(new EmptyCredentials(), new EmptyCredentials(), "tenantId", AzureEnvironment.AzureGlobalCloud));
            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            this.azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();

            Environment.SetEnvironmentVariable("APPSETTING_AnalyticsQueryTimeoutInMinutes", "15");
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientForApplicationInsightsResourceThenTheCorrectClientIsCreated()
        {
            var resource = new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName);
            this.azureResourceManagerClientMock
                .Setup(m => m.GetApplicationInsightsAppIdAsync(resource, default(CancellationToken)))
                .ReturnsAsync("testAppId");

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            LogAnalyticsClient client = await factory.CreateLogAnalyticsClientAsync(resource, default(CancellationToken)) as LogAnalyticsClient;

            Assert.IsNotNull(client);
            Assert.AreEqual(new Uri("https://api.applicationinsights.io/v1/apps/testAppId/query"), client.QueryUri, "Wrong query URI");
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientForLogAnalyticsWorkspaceResourceThenTheCorrectClientIsCreated()
        {
            var resource = new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, ResourceName);
            this.azureResourceManagerClientMock
                .Setup(m => m.GetLogAnalyticsWorkspaceIdAsync(resource, default(CancellationToken)))
                .ReturnsAsync("testWorkspaceId");

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            LogAnalyticsClient client = await factory.CreateLogAnalyticsClientAsync(resource, default(CancellationToken)) as LogAnalyticsClient;

            Assert.IsNotNull(client);
            Assert.AreEqual(new Uri("https://api.loganalytics.io/v1/workspaces/testWorkspaceId/query"), client.QueryUri, "Wrong query URI");
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientForSomeResourceThenTheCorrectClientIsCreated()
        {
            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName);

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            LogAnalyticsClient client = await factory.CreateLogAnalyticsClientAsync(resource, default(CancellationToken)) as LogAnalyticsClient;

            Assert.IsNotNull(client);
            Assert.AreEqual(
                new Uri($"https://api.loganalytics.io/v1/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Compute/virtualMachines/{ResourceName}/query"),
                client.QueryUri,
                "Wrong query URI");
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
    }
}