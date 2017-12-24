namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.SmartSignals.Scheduler;
    using Microsoft.SmartSignals.Scheduler.Publisher;
    using Microsoft.SmartSignals.Scheduler.SignalRunTracker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ScheduleFlowTest
    {
        private Mock<ISmartSignalConfigurationStore> configurationStoreMock;
        private Mock<ISignalRunsTracker> signalRunTrackerMock;
        private Mock<IAnalysisExecuter> analysisExecuterMock;
        private Mock<IDetectionPublisher> detectionPublisherMock;
        private Mock<IAzureResourceManagerClient> azureResourceManagerClientMock;

        private ScheduleFlow scheduleFlow;

        [TestInitialize]
        public void Setup()
        {
            var tracerMock = new Mock<ITracer>();
            this.configurationStoreMock = new Mock<ISmartSignalConfigurationStore>();
            this.signalRunTrackerMock = new Mock<ISignalRunsTracker>();
            this.analysisExecuterMock = new Mock<IAnalysisExecuter>();
            this.detectionPublisherMock = new Mock<IDetectionPublisher>();
            this.azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();

            this.scheduleFlow = new ScheduleFlow(
                tracerMock.Object,
                this.configurationStoreMock.Object,
                this.signalRunTrackerMock.Object,
                this.analysisExecuterMock.Object,
                this.detectionPublisherMock.Object,
                this.azureResourceManagerClientMock.Object);
        }

        [TestMethod]
        public async Task WhenSignalExecutionThrowsExceptionThenNextSignalIsProcessed()
        {
            // Create signal execution information to be returned from the job tracker
            var signalExecution1 = new SignalExecutionInfo
            {
                SignalId = "1",
                AnalysisStartTime = DateTime.UtcNow.AddHours(-1),
                AnalysisEndTime = DateTime.UtcNow
            };
            var signalExecution2 = new SignalExecutionInfo
            {
                SignalId = "2",
                AnalysisStartTime = DateTime.UtcNow.AddHours(-1),
                AnalysisEndTime = DateTime.UtcNow
            };
            var signalExecutions = new List<SignalExecutionInfo> { signalExecution1, signalExecution2 };

            this.signalRunTrackerMock.Setup(m => m.GetSignalsToRunAsync(It.IsAny<IList<SmartSignalConfiguration>>())).ReturnsAsync(signalExecutions);

            this.azureResourceManagerClientMock.Setup(m => m.GetAllSubscriptionIds(It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "someSubscriptionId" });

            // first signal execution throws exception and the second one returns detections
            const string DetectionTitle = "someTitle";
            this.analysisExecuterMock.SetupSequence(m => m.ExecuteSignalAsync(It.IsAny<SignalExecutionInfo>(), It.Is<IList<string>>(lst => lst.First() == "someSubscriptionId")))
                .Throws(new Exception())
                .ReturnsAsync(new List<SmartSignalDetection> { new TestDetection(DetectionTitle) });

            await this.scheduleFlow.RunAsync();

            this.configurationStoreMock.Verify(m => m.GetAllSmartSignalConfigurationsAsync(), Times.Once);
            
            // Verify that these were called only once since the first signal execution throwed exception
            this.detectionPublisherMock.Verify(m => m.PublishDetections("2", It.Is<IList<SmartSignalDetection>>(lst => lst.Count == 1 && lst.First().Title == DetectionTitle)), Times.Once);
            this.signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(It.IsAny<SignalExecutionInfo>()), Times.Once());
            this.signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(signalExecution2));
        }

        [TestMethod]
        public async Task WhenThereAreSignalsToRunThenAllSiganlDetectionsArePublished()
        {
            // Create signal execution information to be returned from the job tracker
            var signalExecution1 = new SignalExecutionInfo
            {
                SignalId = "1",
                AnalysisStartTime = DateTime.UtcNow.AddHours(-1),
                AnalysisEndTime = DateTime.UtcNow
            };
            var signalExecution2 = new SignalExecutionInfo
            {
                SignalId = "2",
                AnalysisStartTime = DateTime.UtcNow.AddHours(-1),
                AnalysisEndTime = DateTime.UtcNow
            };
            var signalExecutions = new List<SignalExecutionInfo> { signalExecution1, signalExecution2 };

            this.signalRunTrackerMock.Setup(m => m.GetSignalsToRunAsync(It.IsAny<IList<SmartSignalConfiguration>>())).ReturnsAsync(signalExecutions);

            this.azureResourceManagerClientMock.Setup(m => m.GetAllSubscriptionIds(It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "someSubscriptionId" });

            // each signal execution returns detections
            this.analysisExecuterMock.Setup(m => m.ExecuteSignalAsync(It.IsAny<SignalExecutionInfo>(), It.Is<IList<string>>(lst => lst.First() == "someSubscriptionId")))
                .ReturnsAsync(new List<SmartSignalDetection> { new TestDetection("title") });

            await this.scheduleFlow.RunAsync();

            // Verify detections were published and signal tracker was updated for each signal execution
            this.configurationStoreMock.Verify(m => m.GetAllSmartSignalConfigurationsAsync(), Times.Once);
            this.detectionPublisherMock.Verify(m => m.PublishDetections(It.IsAny<string>(), It.IsAny<IList<SmartSignalDetection>>()), Times.Exactly(2));
            this.signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(It.IsAny<SignalExecutionInfo>()), Times.Exactly(2));
        }

        private class TestDetection : SmartSignalDetection
        {
            public TestDetection(string title)
            {
                this.Title = title;
            }
            
            public override string Title { get; }
        }
    }
}
