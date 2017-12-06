namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
        private Mock<ISmartSignalConfigurationStore> _configurationStoreMock;
        private Mock<ISignalRunsTracker> _signalRunTrackerMock;
        private Mock<IAnalysisExecuter> _analysisExecuterMock;
        private Mock<IDetectionPublisher> _detectionPublisherMock;

        private ScheduleFlow _scheduleFlow;

        [TestInitialize]
        public void Setup()
        {
            var tracerMock = new Mock<ITracer>();
            _configurationStoreMock = new Mock<ISmartSignalConfigurationStore>();
            _signalRunTrackerMock = new Mock<ISignalRunsTracker>();
            _analysisExecuterMock = new Mock<IAnalysisExecuter>();
            _detectionPublisherMock = new Mock<IDetectionPublisher>();

            _scheduleFlow = new ScheduleFlow(tracerMock.Object, _configurationStoreMock.Object, _signalRunTrackerMock.Object, _analysisExecuterMock.Object, _detectionPublisherMock.Object);
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

            _signalRunTrackerMock.Setup(m => m.GetSignalsToRunAsync(It.IsAny<IList<SmartSignalConfiguration>>())).ReturnsAsync(signalExecutions);

            // first signal execution throws exception and the second one returns detections
            const string detectionTitle = "someTitle";
            _analysisExecuterMock.SetupSequence(m => m.ExecuteSignalAsync(It.IsAny<SignalExecutionInfo>(), It.IsAny<IList<string>>()))
                .Throws(new Exception())
                .ReturnsAsync(new List<SmartSignalDetection> { new TestDetection(detectionTitle) });

            await _scheduleFlow.RunAsync();

            _configurationStoreMock.Verify(m => m.GetAllSmartSignalConfigurationsAsync(), Times.Once);
            
            // Verify that these were called only once since the first signal execution throwed exception
            _detectionPublisherMock.Verify(m => m.PublishDetections("2", It.Is<IList<SmartSignalDetection>>(lst => lst.Count == 1 && lst.First().Title == detectionTitle)), Times.Once);
            _signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(It.IsAny<SignalExecutionInfo>()), Times.Once());
            _signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(signalExecution2));
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

            _signalRunTrackerMock.Setup(m => m.GetSignalsToRunAsync(It.IsAny<IList<SmartSignalConfiguration>>())).ReturnsAsync(signalExecutions);

            // each signal execution returns detections
            _analysisExecuterMock.Setup(m => m.ExecuteSignalAsync(It.IsAny<SignalExecutionInfo>(), It.IsAny<IList<string>>()))
                .ReturnsAsync(new List<SmartSignalDetection> { new TestDetection("title") });

            await _scheduleFlow.RunAsync();

            // Verify detections were published and signal tracker was updated for each signal execution
            _configurationStoreMock.Verify(m => m.GetAllSmartSignalConfigurationsAsync(), Times.Once);
            _detectionPublisherMock.Verify(m => m.PublishDetections(It.IsAny<string>(), It.IsAny<IList<SmartSignalDetection>>()), Times.Exactly(2));
            _signalRunTrackerMock.Verify(m => m.UpdateSignalRunAsync(It.IsAny<SignalExecutionInfo>()), Times.Exactly(2));
        }
    }

    public class TestDetection : SmartSignalDetection
    {
        public override string Title { get; }

        public TestDetection(string title)
        {
            Title = title;
        }
    }
}
