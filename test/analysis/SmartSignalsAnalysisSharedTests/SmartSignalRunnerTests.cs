namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis.DetectionPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartSignalRunnerTests
    {
        private SmartSignalMetadata smartSignalMetadata;
        private List<string> resourceIds;
        private SmartSignalRequest request;
        private TestSignal signal;
        private Mock<ITracer> tracerMock;
        private Mock<ISmartSignalsRepository> smartSignalsRepositoryMock;
        private Mock<ISmartSignalLoader> smartSignalLoaderMock;
        private Mock<ISmartSignalAnalysisServicesFactory> smartSignalAnalysisServicesFactoryMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();

            this.smartSignalMetadata = new SmartSignalMetadata("1", "Test signal", "Test signal description", "1.0", "assembly", "class");

            this.smartSignalsRepositoryMock = new Mock<ISmartSignalsRepository>();
            this.smartSignalsRepositoryMock
                .Setup(x => x.ReadSignalMetadataAsync(It.IsAny<string>()))
                .ReturnsAsync(() => this.smartSignalMetadata);

            this.smartSignalAnalysisServicesFactoryMock = new Mock<ISmartSignalAnalysisServicesFactory>();

            this.signal = new TestSignal();

            this.smartSignalLoaderMock = new Mock<ISmartSignalLoader>();
            this.smartSignalLoaderMock
                .Setup(x => x.LoadSignalAsync(this.smartSignalMetadata))
                .ReturnsAsync(() => this.signal);

            this.resourceIds = new List<string>() { "resourceId" };

            this.request = new SmartSignalRequest(this.resourceIds, "1", DateTime.Now.AddDays(-1), DateTime.Now, new SmartSignalSettings());
        }

        [TestMethod]
        public async Task WhenRunningSignalThenTheCorrectDetectionIsReturned()
        {
            // Run teh signal and validate results
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartSignalLoaderMock.Object, this.smartSignalAnalysisServicesFactoryMock.Object, this.tracerMock.Object);
            List<SmartSignalDetectionPresentation> detections = await runner.RunAsync(this.request, default(CancellationToken));
            Assert.IsNotNull(detections, "Detections list is null");
            Assert.AreEqual(1, detections.Count);
            Assert.AreEqual("Test title", detections.Single().Title);
            Assert.AreEqual("Summary value", detections.Single().Summary.Value);
        }

        [TestMethod]
        public void WhenRunningSignalThenCancellationIsHandledGracefully()
        {
            // Notify the signal that it should get stuck and wait for cancellation
            this.signal.ShouldStuck = true;

            // Run the signal asynchronously
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartSignalLoaderMock.Object, this.smartSignalAnalysisServicesFactoryMock.Object, this.tracerMock.Object);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.RunAsync(this.request, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.signal.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            try
            {
                t.Wait(TimeSpan.FromSeconds(10));
            }
            catch (AggregateException e) when (e.InnerExceptions.Single() is TaskCanceledException)
            {
                Assert.IsTrue(this.signal.WasCanceled, "The signal was not canceled!");
            }
        }

        private class TestSignal : ISmartSignal
        {
            public bool ShouldStuck { get; set; }

            public bool IsRunning { get; private set; }

            public bool WasCanceled { get; private set; }

            public async Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                Assert.IsNotNull(targetResources, "Resources list is null");
                Assert.AreEqual(1, targetResources.Count);
                Assert.AreEqual(ResourceType.VirtualMachine, targetResources.Single().ResourceType);

                if (this.ShouldStuck)
                {
                    try
                    {
                        await Task.Delay(int.MaxValue, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this.WasCanceled = true;
                        throw;
                    }
                }

                return await Task.FromResult(new List<SmartSignalDetection>()
                {
                    new TestSignalDetection()
                });
            }
        }

        private class TestSignalDetection : SmartSignalDetection
        {
            public override string Title { get; } = "Test title";

            [DetectionPresentation(DetectionPresentationSection.Property, "Summary title", InfoBalloon = "Summary info", Component = DetectionPresentationComponent.Summary)]
            public string Summary { get; } = "Summary value";
        }
    }
}