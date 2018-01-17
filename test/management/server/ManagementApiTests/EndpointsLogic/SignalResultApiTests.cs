//-----------------------------------------------------------------------
// <copyright file="SignalResultApiTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.SignalResultPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class SignalResultApiTests
    {
        private readonly DateTime startTime = new DateTime(2018, 1, 1);
        private readonly DateTime endTime = new DateTime(2018, 1, 2);

        private Mock<IApplicationInsightsClient> applicationInsightClientMock;
        private ISignalResultApi signalResultApi;

        [TestInitialize]
        public void Initialize()
        {
            this.applicationInsightClientMock = new Mock<IApplicationInsightsClient>();
            this.signalResultApi = new SignalResultApi(this.applicationInsightClientMock.Object);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsHappyFlow()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(this.GetApplicationInsightsEvents());

            ListSmartSignalsResultsResponse response = await this.signalResultApi.GetAllSmartSignalResultsAsync(this.startTime, this.endTime, CancellationToken.None);

            Assert.AreEqual(1, response.SignalsResults.Count);
            Assert.AreEqual("someId", response.SignalsResults.First().Id);
            Assert.AreEqual("someTitle", response.SignalsResults.First().Title);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsButFailedToQueryApplicationInsightsThenThrowException()
        {
            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                             .ThrowsAsync(new ApplicationInsightsClientException("some message"));

            try
            {
                await this.signalResultApi.GetAllSmartSignalResultsAsync(this.startTime, this.endTime, CancellationToken.None);
            }
            catch (SmartSignalsManagementApiException)
            {
                return;
            }

            Assert.Fail("A management exception should have been thrown in case failing to query application insights");
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsResultsButFailedToDeserializeResultsThenThrowException()
        {
            var returnedApplicationInsightsEvents = this.GetApplicationInsightsEvents();
            returnedApplicationInsightsEvents.Add(new ApplicationInsightsEvent()
            {
                Id = "someId",
                Timestamp = DateTime.UtcNow,
                CustomDimensions = new Dictionary<string, string>()
                {
                    {
                        "ResultItem", "someCorruptedValue"
                    }
                }
            });

            this.applicationInsightClientMock.Setup(ai => ai.GetCustomEventsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(returnedApplicationInsightsEvents);

            try
            {
                await this.signalResultApi.GetAllSmartSignalResultsAsync(this.startTime, this.endTime, CancellationToken.None);
            }
            catch (SmartSignalsManagementApiException)
            {
                return;
            }

            Assert.Fail("A management exception should have been thrown in case failing to de-serialize signals results");
        }

        private List<ApplicationInsightsEvent> GetApplicationInsightsEvents()
        {
            var signalResult = new SmartSignalResultItemPresentation(
                                    id: "someId",
                                    title: "someTitle",
                                    summary: new SmartSignalResultItemPresentationSummary(
                                                "3980",
                                                "Maximum Request Count for the application", 
                                                new SmartSignalResultItemPresentationProperty("Bar Chart", "Perf | where TimeGenerated >= ago(1h) | where CounterName == \'% Processor Time\'|", ResultItemPresentationSection.Chart, string.Empty)),
                                    resourceId: "/subscriptions/b4b7d4c1-8c25-4da3-bf1c-e50f647a8130/resourceGroups/asafst/providers/Microsoft.Insights/components/deepinsightsdailyreports",
                                    correlationHash: "93e9a62b1e1a0dca5d9d63cc7e9aae71edb9988aa6f1dfc3b85e71b0f57d2819",
                                    signalId: "SampleSignal",
                                    signalName: "SampleSignal",
                                    analysisTimestamp: DateTime.UtcNow,
                                    analysisWindowSizeInMinutes: 5,
                                    properties: new List<SmartSignalResultItemPresentationProperty>(),
                                    rawProperties: new Dictionary<string, string>());

            return new List<ApplicationInsightsEvent>()
            {
                new ApplicationInsightsEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CustomDimensions = new Dictionary<string, string>()
                    {
                        {
                            "ResultItem", JsonConvert.SerializeObject(signalResult)
                        }
                    }
                }
            };
        }
    }
}
