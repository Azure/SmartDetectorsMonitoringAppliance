//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsClientTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.AIClient
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ApplicationInsightsClientTests
    {
        private const string ApplicationId = "someApplicationId";

        private Mock<IHttpClientWrapper> httpClientMock;
        private IApplicationInsightsClient applicationInsightsClient;

        [TestInitialize]
        public void Initialize()
        {
            this.httpClientMock = new Mock<IHttpClientWrapper>();
            this.applicationInsightsClient = new ApplicationInsightsClient(ApplicationId, this.httpClientMock.Object);
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightsForCustomEventsHappyFlow()
        {
            HttpRequestMessage requestMessage = null;

            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) => requestMessage = message)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                              {
                                Content = new StringContent(File.ReadAllText("AIClient\\AIEndpointResponses\\SuccessfulResponse.txt"))
                              });
                
            // Get data using AI client
            var customEvents = await this.applicationInsightsClient.GetCustomEventsAsync();

            // Verify we got the required amount of events
            Assert.AreEqual(10, customEvents.Count());

            // Verify the executed url was the correct one
            Assert.AreEqual("https://api.applicationinsights.io/v1/apps/someApplicationId/events/customEvents", requestMessage.RequestUri.ToString());
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsWithStartTimeThenCorrectRequestRaised()
        {
            HttpRequestMessage requestMessage = null;
            DateTime queryStartTime = DateTime.UtcNow.AddDays(-1);

            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) => requestMessage = message)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("AIClient\\AIEndpointResponses\\SuccessfulResponse.txt"))
                });

            // Get data using AI client
            var customEvents = await this.applicationInsightsClient.GetCustomEventsAsync(queryStartTime);

            // Verify we got the required amount of events
            Assert.AreEqual(10, customEvents.Count());

            // Verify the executed url was the correct one
            Assert.AreEqual(
                   $"https://api.applicationinsights.io/v1/apps/someApplicationId/events/customEvents?$filter=timestamp ge {queryStartTime}", 
                   requestMessage.RequestUri.ToString());
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsWithStartTimeAndEndTimeThenCorrectRequestRaised()
        {
            HttpRequestMessage requestMessage = null;
            DateTime queryStartTime = DateTime.UtcNow.AddDays(-1);
            DateTime queryEndTime = DateTime.UtcNow;

            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) => requestMessage = message)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("AIClient\\AIEndpointResponses\\SuccessfulResponse.txt"))
                });

            // Get data using AI client
            var customEvents = await this.applicationInsightsClient.GetCustomEventsAsync(queryStartTime, queryEndTime);

            // Verify we got the required amount of events
            Assert.AreEqual(10, customEvents.Count());

            // Verify the executed url was the correct one
            Assert.AreEqual(
                   $"https://api.applicationinsights.io/v1/apps/someApplicationId/events/customEvents?$filter=timestamp ge {queryStartTime} AND timestamp le {queryEndTime}",
                   requestMessage.RequestUri.ToString());
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsWithStartTimeAndEndTimeButEndTimeIsBeforeStartTimeThenExceptionThrown()
        {
            DateTime queryStartTime = DateTime.UtcNow;
            DateTime queryEndTime = DateTime.UtcNow.AddDays(-1);

            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("AIClient\\AIEndpointResponses\\SuccessfulResponse.txt"))
                });

            try
            {
                // Get data using AI client
                await this.applicationInsightsClient.GetCustomEventsAsync(queryStartTime, queryEndTime);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            Assert.Fail("End time is after Start time and therfore it should have throw an exception");
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsButEndpointReturnsCorruptedResultsThenExceptionThrown()
        {
            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Corrupted Result")
                });

            try
            {
                // Get data using AI client
                await this.applicationInsightsClient.GetCustomEventsAsync();
            }
            catch (ApplicationInsightsClientException)
            {
                return;
            }
            
            Assert.Fail("Corrupted results should throw an exception");
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsButEndpointReturnsNotSuccessStatusCodeThenExceptionThrownWithResponseContent()
        {
            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Response content")
                });

            try
            {
                // Get data using AI client
                await this.applicationInsightsClient.GetCustomEventsAsync();
            }
            catch (ApplicationInsightsClientException e)
            {
                Assert.IsTrue(e.Message.Contains("Response content"));

                return;
            }

            Assert.Fail("Not success status code should throw an exception");
        }

        [TestMethod]
        public async Task WhenQueryApplicationInsightForCustomEventsButHttpClientThrowsExceptionTheWrappedExceptionThrown()
        {
            // Configure mock to return the successful response
            this.httpClientMock.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            try
            {
                // Get data using AI client
                await this.applicationInsightsClient.GetCustomEventsAsync();
            }
            catch (ApplicationInsightsClientException)
            {
                return;
            }

            Assert.Fail("When HttpClient throws an exception then the client should throw an exception");
        }
    }
}
