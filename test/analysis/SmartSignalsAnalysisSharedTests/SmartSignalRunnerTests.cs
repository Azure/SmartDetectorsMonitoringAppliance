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
        private Mock<IAzureResourceManagerClient> azureResourceManagerClientMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.TestInitialize(ResourceType.VirtualMachine, ResourceType.VirtualMachine);
        }

        [TestMethod]
        public async Task WhenRunningSignalThenTheCorrectDetectionIsReturned()
        {
            // Run the signal and validate results
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartSignalLoaderMock.Object, this.smartSignalAnalysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.tracerMock.Object);
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
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartSignalLoaderMock.Object, this.smartSignalAnalysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.tracerMock.Object);
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

        [TestMethod]
        public async Task WhenRunningSignalWithSupportedTypeThenTheCorrectResultsAreReturned()
        {
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.Subscription, false);
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.ResourceGroup, false);
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.VirtualMachine, false);
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.ResourceGroup, false);
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.VirtualMachine, false);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.VirtualMachine, false);
        }

        [TestMethod]
        public async Task WhenRunningSignalWithUnsupportedTypeThenAnExceptionIsThrown()
        {
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.Subscription, true);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.Subscription, true);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.ResourceGroup, true);
        }

        private async Task RunSignalWithResourceTypes(ResourceType requestResourceType, ResourceType signalResourceType, bool shouldFail)
        {
            this.TestInitialize(requestResourceType, signalResourceType);
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartSignalLoaderMock.Object, this.smartSignalAnalysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.tracerMock.Object);
            try
            {
                List<SmartSignalDetectionPresentation> detections = await runner.RunAsync(this.request, default(CancellationToken));
                if (shouldFail)
                {
                    Assert.Fail("An exception should have been thrown - resource types are not compatible");
                }

                Assert.AreEqual(1, detections.Count);
            }
            catch (IncompatibleResourceTypesException)
            {
                if (!shouldFail)
                {
                    throw;
                }
            }
        }

        private void TestInitialize(ResourceType requestResourceType, ResourceType signalResourceType)
        {
            this.tracerMock = new Mock<ITracer>();

            this.resourceIds = new List<string>() { requestResourceType.ToString() };

            this.request = new SmartSignalRequest(this.resourceIds, "1", DateTime.Now.AddDays(-1), DateTime.Now, new SmartSignalSettings());

            this.smartSignalMetadata = new SmartSignalMetadata("1", "Test signal", "Test signal description", "1.0", "assembly", "class", new List<ResourceType>() { signalResourceType });

            this.smartSignalsRepositoryMock = new Mock<ISmartSignalsRepository>();
            this.smartSignalsRepositoryMock
                .Setup(x => x.ReadSignalMetadataAsync(It.IsAny<string>()))
                .ReturnsAsync(() => this.smartSignalMetadata);

            this.smartSignalAnalysisServicesFactoryMock = new Mock<ISmartSignalAnalysisServicesFactory>();

            this.signal = new TestSignal { ExpectedResourceType = signalResourceType };

            this.smartSignalLoaderMock = new Mock<ISmartSignalLoader>();
            this.smartSignalLoaderMock
                .Setup(x => x.LoadSignalAsync(this.smartSignalMetadata))
                .ReturnsAsync(() => this.signal);

            this.azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();
            this.azureResourceManagerClientMock
                .Setup(x => x.GetResourceIdentifier(It.IsAny<string>()))
                .Returns((string resourceId) =>
                {
                    ResourceType resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), resourceId);
                    if (resourceType == ResourceType.Subscription)
                    {
                        return ResourceIdentifier.Create("subscriptionId");
                    }
                    else if (resourceType == ResourceType.ResourceGroup)
                    {
                        return ResourceIdentifier.Create("subscriptionId", "resourceGroupName");
                    }
                    else
                    {
                        return ResourceIdentifier.Create(resourceType, "subscriptionId", "resourceGroupName", "resourceName");
                    }
                });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourceGroupsInSubscription(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { ResourceIdentifier.Create(subscriptionId, "resourceGroupName") });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscription(It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { ResourceIdentifier.Create(ResourceType.VirtualMachine, subscriptionId, "resourceGroupName", "resourceName") });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInResourceGroup(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { ResourceIdentifier.Create(ResourceType.VirtualMachine, subscriptionId, resourceGroupName, "resourceName") });
        }

        private class TestSignal : ISmartSignal
        {
            public bool ShouldStuck { private get; set; }

            public bool IsRunning { get; private set; }

            public bool WasCanceled { get; private set; }

            public ResourceType ExpectedResourceType { private get; set; }

            public async Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                Assert.IsNotNull(targetResources, "Resources list is null");
                Assert.AreEqual(1, targetResources.Count);
                Assert.AreEqual(this.ExpectedResourceType, targetResources.Single().ResourceType);

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