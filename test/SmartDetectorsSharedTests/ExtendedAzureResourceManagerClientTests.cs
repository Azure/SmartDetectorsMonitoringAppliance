//-----------------------------------------------------------------------
// <copyright file="ExtendedAzureResourceManagerClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class ExtendedAzureResourceManagerClientTests
    {
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<ICredentialsFactory> credentialsFactoryMock;
        private Mock<ITracer> tracerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            this.tracerMock = new Mock<ITracer>();

            this.credentialsFactoryMock = new Mock<ICredentialsFactory>();
            this.credentialsFactoryMock.Setup(x => x.CreateServiceClientCredentials(It.IsAny<string>())).Returns(new EmptyCredentials());
            this.credentialsFactoryMock.Setup(x => x.CreateAzureCredentials(It.IsAny<string>())).Returns(() => new AzureCredentials(new EmptyCredentials(), new EmptyCredentials(), "tenantId", AzureEnvironment.AzureGlobalCloud));
        }

        [TestMethod]
        public async Task WhenCallingExecuteArmQueryAsyncThenTheFlowIsCorrect()
        {
            ResourceIdentifier sqlResource = new ResourceIdentifier(ResourceType.SqlServer, "subscription", "resourceGroup", "server1");

            string url = "https://management.azure.com/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Sql/servers/server1/databases?api-version=3.3";
            string nextLink1 = "https://management.azure.com/next1";
            string nextLink2 = "https://management.azure.com/next2";

            this.SetupHttpClientWrapper(url, nextLink1, "db1", "db2");
            this.SetupHttpClientWrapper(nextLink1, nextLink2, "db3", "db4", "db5");
            this.SetupHttpClientWrapper(nextLink2, string.Empty, "db6");

            IAzureResourceManagerClient azureResourceManagerClient = new ExtendedAzureResourceManagerClient(this.httpClientWrapperMock.Object, this.credentialsFactoryMock.Object, this.tracerMock.Object);
            List<JObject> databases = await azureResourceManagerClient.ExecuteArmQueryAsync(sqlResource, "/databases", "api-version=3.3", CancellationToken.None);

            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.ToString() == url), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.ToString() == nextLink1), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
            this.httpClientWrapperMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.ToString() == nextLink2), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(6, databases.Count);
            Assert.AreEqual("db1", databases[0]["DBName"]);
            Assert.AreEqual("db2", databases[1]["DBName"]);
            Assert.AreEqual("db3", databases[2]["DBName"]);
            Assert.AreEqual("db4", databases[3]["DBName"]);
            Assert.AreEqual("db5", databases[4]["DBName"]);
            Assert.AreEqual("db6", databases[5]["DBName"]);
        }

        private void SetupHttpClientWrapper(string expectedUrl, string nextLink, params string[] results)
        {
            this.httpClientWrapperMock
                .Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.ToString() == expectedUrl), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    JObject responseObject = new JObject
                    {
                        ["nextLink"] = nextLink,
                        ["value"] = JArray.FromObject(results.Select(x => new { DBName = x }))
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseObject.ToString())
                    };
                });
        }
    }
}