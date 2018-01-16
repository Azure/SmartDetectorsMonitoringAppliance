//-----------------------------------------------------------------------
// <copyright file="AnalysisServicesFactoryTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Common;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AnalysisServicesFactoryTests
    {
        private const string SubscriptionId = "subscriptionId";
        private const string ResourceGroupName = "resourceGroupName";
        private const string ResourceName = "resourceName";
        private const string ApplicationId = "applicationId";
        private static readonly List<string> WorkspaceIds = new List<string>() { "workspaceId1", "workspaceId2", "workspaceId3" };
        private static readonly List<string> WorkspaceNames = new List<string>() { "workspaceName1", "workspaceName2", "workspaceName3" };
        private static readonly List<ResourceIdentifier> Workspaces = WorkspaceNames.Select(name => ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, name)).ToList();

        private Mock<ITracer> tracerMock;
        private Mock<ICredentialsFactory> credentialsFactoryMock;
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<IAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();
            this.credentialsFactoryMock = new Mock<ICredentialsFactory>();
            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();

            this.azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();
            this.azureResourceManagerClientMock
                .Setup(x => x.GetApplicationInsightsAppIdAsync(It.Is<ResourceIdentifier>(identifier => identifier.ResourceType == ResourceType.ApplicationInsights), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApplicationId);
            this.azureResourceManagerClientMock
                .Setup(x => x.GetLogAnalyticsWorkspaceIdAsync(It.Is<ResourceIdentifier>(identifier => identifier.ResourceType == ResourceType.LogAnalytics), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceIdentifier resourceIdentifier, CancellationToken cancellationToken) =>
                {
                    int idx = WorkspaceNames.FindIndex(name => name == resourceIdentifier.ResourceName);
                    return WorkspaceIds[idx];
                });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId, It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Workspaces);
            this.azureResourceManagerClientMock
                .Setup(x => x.GetResourceId(It.IsAny<ResourceIdentifier>()))
                .Returns((ResourceIdentifier resourceIdentifier) => resourceIdentifier.ResourceName);

            Environment.SetEnvironmentVariable("APPSETTING_AnalyticsQueryTimeoutInMinutes", "15");
        }

        [TestMethod]
        public async Task WhenCreatingApplicationInsightsClientForTheCorrectResourcesThenTheCorrectClientIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            ITelemetryDataClient client = await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
            Assert.AreEqual(typeof(ApplicationInsightsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            Assert.AreEqual(ApplicationId, GetPrivateFieldValue<string>(client, "applicationId"), "Wrong application Id");
            CollectionAssert.AreEqual(new[] { ResourceName }, GetPrivateFieldValue<ICollection>(client, "applicationsResourceIds"));
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingApplicationInsightsClientForMixedResourcesThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName + "111"),
                ResourceIdentifier.Create(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientForLogAnalyticsResourcesThenTheCorrectClientIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[1])
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            ITelemetryDataClient client = await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
            Assert.AreEqual(typeof(LogAnalyticsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            Assert.AreEqual(WorkspaceIds[0], GetPrivateFieldValue<string>(client, "workspaceId"), "Wrong application Id");
            CollectionAssert.AreEqual(new[] { WorkspaceNames[0], WorkspaceNames[1] }, GetPrivateFieldValue<ICollection>(client, "workspacesResourceIds"), "Wrong workspace names");
        }

        [TestMethod]
        [ExpectedException(typeof(TelemetryDataClientCreationException))]
        public async Task WhenCreatingLogAnalyticsClientForMixedResourcesThenAnExceptionIsThrown()
        {
            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                ResourceIdentifier.Create(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenCreatingLogAnalyticsClientForGeneralResourcesThenAllWorkspacesAreUsed()
        {
            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                ResourceIdentifier.Create(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);
            ITelemetryDataClient client = await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
            Assert.AreEqual(typeof(LogAnalyticsTelemetryDataClient), client.GetType(), "Wrong telemetry data client type created");
            Assert.IsTrue(WorkspaceIds.Contains(GetPrivateFieldValue<string>(client, "workspaceId")), "Wrong workspace Id");
            CollectionAssert.AreEqual(WorkspaceNames, GetPrivateFieldValue<ICollection>(client, "workspacesResourceIds"));
        }

        [TestMethod]
        public async Task WhenCreatingClientForEmptyResourcesThenAnExceptionIsThrown()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);

            try
            {
                await factory.CreateApplicationInsightsTelemetryDataClientAsync(new List<ResourceIdentifier>(), default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (TelemetryDataClientCreationException)
            {
            }

            try
            {
                await factory.CreateLogAnalyticsTelemetryDataClientAsync(new List<ResourceIdentifier>(), default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (TelemetryDataClientCreationException)
            {
            }
        }

        [TestMethod]
        public async Task WhenCreatingClientForResourcesWithMultipleSubscriptionsThenAllWorkspacesAreReturned()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);

            var resources = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.VirtualMachine, SubscriptionId + "1", ResourceGroupName + "1", ResourceName + "1"),
                ResourceIdentifier.Create(ResourceType.VirtualMachine, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "2")
            };

            var workspaces = new List<ResourceIdentifier>()
            {
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId + "1", ResourceGroupName + "1", ResourceName + "1"),
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "2"),
                ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "3")
            };

            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "1", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ResourceIdentifier>() { workspaces[0] });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "2", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ResourceIdentifier>() { workspaces[1], workspaces[2] });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetLogAnalyticsWorkspaceIdAsync(It.Is<ResourceIdentifier>(identifier => identifier.ResourceType == ResourceType.LogAnalytics), It.IsAny<CancellationToken>()))
                .ReturnsAsync("workspaceId");

            var client = await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));

            this.azureResourceManagerClientMock.Verify(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "1", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()), Times.Once);
            this.azureResourceManagerClientMock.Verify(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "2", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()), Times.Once);
            CollectionAssert.AreEqual(workspaces.Select(x => x.ResourceName).ToList(), GetPrivateFieldValue<ICollection>(client, "workspacesResourceIds"));
        }

        [TestMethod]
        public async Task WhenCreatingClientForTooManyResourcesThenAnExceptionIsThrown()
        {
            const int TooManyResourcesCount = 11;
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object);

            try
            {
                List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                    .Select(i => ResourceIdentifier.Create(ResourceType.ApplicationInsights, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
                await factory.CreateApplicationInsightsTelemetryDataClientAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (TelemetryDataClientCreationException)
            {
            }

            try
            {
                List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                    .Select(i => ResourceIdentifier.Create(ResourceType.LogAnalytics, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
                await factory.CreateLogAnalyticsTelemetryDataClientAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (TelemetryDataClientCreationException)
            {
            }
        }

        private static T GetPrivateFieldValue<T>(object obj, string propertyName)
        {
            return (T)obj.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
        }
    }
}