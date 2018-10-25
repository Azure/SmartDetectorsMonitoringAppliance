//-----------------------------------------------------------------------
// <copyright file="QueryRunInfoProviderTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class QueryRunInfoProviderTests
    {
        private const string SubscriptionId = "subscriptionId";
        private const string ResourceGroupName = "resourceGroupName";
        private const string ResourceName = "resourceName";
        private const string ApplicationId = "applicationId";
        private static readonly List<string> WorkspaceIds = new List<string>() { "workspaceId1", "workspaceId2", "workspaceId3" };
        private static readonly List<string> WorkspaceNames = new List<string>() { "workspaceName1", "workspaceName2", "workspaceName3" };
        private static readonly List<ResourceIdentifier> Workspaces = WorkspaceNames.Select(name => new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, name)).ToList();

        private Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();
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
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoForApplicationInsightsResourcesThenTheCorrectInfoIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
            };

            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);

            QueryRunInfo queryRunInfo = await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));

            Assert.IsNotNull(queryRunInfo, "Query run information is null");
            Assert.AreEqual(TelemetryDbType.ApplicationInsights, queryRunInfo.Type, "Wrong telemetry DB type");
            CollectionAssert.AreEqual(new[] { resources.First().ToResourceId() }, queryRunInfo.ResourceIds.ToArray(), "Wrong resource IDs");
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoForMixedResourcesThenAnExceptionIsThrown()
        {
            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);

            try
            {
                var resources = new List<ResourceIdentifier>()
                {
                    new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName + "111"),
                    new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId, ResourceGroupName, ResourceName)
                };

                await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (QueryClientInfoProviderException)
            {
            }

            try
            {
                var resources = new List<ResourceIdentifier>()
                {
                    new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                    new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId, ResourceGroupName, ResourceName)
                };

                await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (QueryClientInfoProviderException)
            {
            }
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoForLogAnalyticsResourcesThenTheCorrectInfoIsCreated()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[1])
            };

            string[] resourceIds = resources.Select(r => r.ToResourceId()).ToArray();
            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);
            QueryRunInfo queryRunInfo = await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));

            Assert.IsNotNull(queryRunInfo, "Query run information is null");
            Assert.AreEqual(TelemetryDbType.LogAnalytics, queryRunInfo.Type, "Wrong telemetry DB type");
            CollectionAssert.AreEqual(resourceIds, queryRunInfo.ResourceIds.ToArray(), "Wrong resource IDs");
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoGeneralResourcesThenAllWorkspacesAreUsed()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[0]),
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[1]),
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId, ResourceGroupName, WorkspaceNames[2])
            };

            string[] array = resources.Select(r => r.ToResourceId()).ToArray();
            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);
            QueryRunInfo queryRunInfo = await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));

            Assert.IsNotNull(queryRunInfo, "Query run information is null");
            Assert.AreEqual(TelemetryDbType.LogAnalytics, queryRunInfo.Type, "Wrong telemetry DB type");
            CollectionAssert.AreEqual(array, queryRunInfo.ResourceIds.ToArray(), "Wrong resource IDs");
        }

        [TestMethod]
        [ExpectedException(typeof(QueryClientInfoProviderException))]
        public async Task WhenCreatingQueryRunInfoForEmptyResourcesThenAnExceptionIsThrown()
        {
            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);
            await provider.GetQueryRunInfoAsync(new List<ResourceIdentifier>(), default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoForResourcesWithMultipleSubscriptionsThenAllWorkspacesAreReturned()
        {
            var resources = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId + "1", ResourceGroupName + "1", ResourceName + "1"),
                new ResourceIdentifier(ResourceType.VirtualMachine, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "2")
            };

            var workspaces = new List<ResourceIdentifier>()
            {
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId + "1", ResourceGroupName + "1", ResourceName + "1"),
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "2"),
                new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId + "2", ResourceGroupName + "2", ResourceName + "3")
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

            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);
            QueryRunInfo queryRunInfo = await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));

            Assert.IsNotNull(queryRunInfo, "Query run information is null");
            this.azureResourceManagerClientMock.Verify(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "1", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()), Times.Once);
            this.azureResourceManagerClientMock.Verify(x => x.GetAllResourcesInSubscriptionAsync(SubscriptionId + "2", It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()), Times.Once);
            CollectionAssert.AreEqual(workspaces.Select(w => w.ToResourceId()).ToArray(), queryRunInfo.ResourceIds.ToArray());
        }

        [TestMethod]
        public async Task WhenCreatingQueryRunInfoForTooManyResourcesThenAnExceptionIsThrown()
        {
            const int TooManyResourcesCount = 11;
            IQueryRunInfoProvider provider = new QueryRunInfoProvider(this.azureResourceManagerClientMock.Object);

            try
            {
                List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                    .Select(i => new ResourceIdentifier(ResourceType.ApplicationInsights, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
                await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (QueryClientInfoProviderException)
            {
            }

            try
            {
                List<ResourceIdentifier> resources = Enumerable.Range(1, TooManyResourcesCount)
                    .Select(i => new ResourceIdentifier(ResourceType.LogAnalytics, SubscriptionId + i, ResourceGroupName + i, ResourceName + i)).ToList();
                await provider.GetQueryRunInfoAsync(resources, default(CancellationToken));
                Assert.Fail("An exception should be thrown");
            }
            catch (QueryClientInfoProviderException)
            {
            }
        }
    }
}