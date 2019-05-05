//-----------------------------------------------------------------------
// <copyright file="ActivityLogClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class ActivityLogClientTests
    {
        private Mock<ITracer> tracerMock;
        private Mock<ICredentialsFactory> credentialsFactoryMock;
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private IExtendedAzureResourceManagerClient azureResourceManagerClient;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();
            this.credentialsFactoryMock = new Mock<ICredentialsFactory>();
            this.credentialsFactoryMock.Setup(x => x.CreateServiceClientCredentials(It.IsAny<string>())).Returns(() => new EmptyCredentials());
            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            this.azureResourceManagerClient = new ExtendedAzureResourceManagerClient(this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenCallingActivityLogClientWithVmResourceTypeThenTheCorrectUriIsCreated()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClient);
            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, "subscriptionId", "resourceGroupName", "resourceName");
            IActivityLogClient client = await factory.CreateActivityLogClientAsync(default(CancellationToken));

            JObject returnValue = new JObject()
            {
                ["value"] = new JArray()
            };

            Uri requestUri = new Uri("https://management.azure.com/subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp ge '2018-04-30 17:00:00Z' and eventTimestamp le '2018-04-30 23:00:00Z' and resourceUri eq '/subscriptions/subscriptionId/resourceGroups/resourceGroupName/providers/Microsoft.Compute/virtualMachines/resourceName");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(returnValue.ToString())
            };

            this.httpClientWrapperMock.Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            await client.GetActivityLogAsync(resource, new DateTime(2018, 04, 30, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 30, 23, 0, 0), CancellationToken.None);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenCallingActivityLogClientWithResourceGroupResourceTypeThenTheCorrectUriIsCreated()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClient);
            var resource = new ResourceIdentifier(ResourceType.ResourceGroup, "subscriptionId", "resourceGroupName", string.Empty);
            IActivityLogClient client = await factory.CreateActivityLogClientAsync(default(CancellationToken));

            JObject returnValue = new JObject()
            {
                ["value"] = new JArray()
            };

            Uri requestUri = new Uri("https://management.azure.com/subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp ge '2018-04-30 17:00:00Z' and eventTimestamp le '2018-04-30 23:00:00Z' and resourceGroupName eq 'resourceGroupName");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(returnValue.ToString())
            };

            this.httpClientWrapperMock.Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            await client.GetActivityLogAsync(resource, new DateTime(2018, 04, 30, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 30, 23, 0, 0), CancellationToken.None);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenCallingActivityLogClientWithSubscriptionResourceTypeThenTheCorrectUriIsCreated()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClient);
            var resource = new ResourceIdentifier(ResourceType.Subscription, "subscriptionId", string.Empty, string.Empty);
            IActivityLogClient client = await factory.CreateActivityLogClientAsync(default(CancellationToken));

            JObject returnValue = new JObject()
            {
                ["value"] = new JArray()
            };

            Uri requestUri = new Uri("https://management.azure.com/subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp ge '2018-04-30 17:00:00Z' and eventTimestamp le '2018-04-30 23:00:00Z'");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(returnValue.ToString())
            };

            this.httpClientWrapperMock.Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            await client.GetActivityLogAsync(resource, new DateTime(2018, 04, 30, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 30, 23, 0, 0), CancellationToken.None);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenCallingActivityLogClientWithMultiplePagesThenTheCorrectResponseIsReturned()
        {
            IAnalysisServicesFactory factory = new AnalysisServicesFactory(this.tracerMock.Object, this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.azureResourceManagerClient);
            var resource = new ResourceIdentifier(ResourceType.Subscription, "subscriptionId", string.Empty, string.Empty);
            IActivityLogClient client = await factory.CreateActivityLogClientAsync(default(CancellationToken));

            string nextLink = "https://management.azure.com/";
            Uri nextLinkUri = new Uri(nextLink);

            JObject returnValue = new JObject
            {
                ["value"] = new JArray
                {
                    new JObject
                    {
                        ["subscriptionId"] = "subId"
                    }
                },
                ["nextLink"] = nextLink
            };

            JObject nextLinkReturnValue = new JObject
            {
                ["value"] = new JArray
                {
                    new JObject
                    {
                        ["subscriptionId"] = "subId2"
                    }
                }
            };

            Uri requestUri = new Uri("https://management.azure.com/subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp ge '2018-04-30 17:00:00Z' and eventTimestamp le '2018-04-30 23:00:00Z'");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(returnValue.ToString())
            };

            HttpResponseMessage nextLinkResponse = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(nextLinkReturnValue.ToString())
            };

            this.httpClientWrapperMock.Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            this.httpClientWrapperMock.Setup(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == nextLinkUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(nextLinkResponse);
            await client.GetActivityLogAsync(resource, new DateTime(2018, 04, 30, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 30, 23, 0, 0), CancellationToken.None);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == requestUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri == nextLinkUri), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
