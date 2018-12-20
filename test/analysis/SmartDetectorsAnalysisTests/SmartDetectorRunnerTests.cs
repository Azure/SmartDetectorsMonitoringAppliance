//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Unity;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using AlertResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.AlertResolutionCheckRequest;
    using AlertResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.AlertResolutionCheckResponse;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsAlertResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertResolutionCheckRequest;
    using ContractsAlertResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertResolutionCheckResponse;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    [SuppressMessage("Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test class - allowed")]
    public class SmartDetectorRunnerTests
    {
        private SmartDetectorPackage smartDetectorPackage;
        private SmartDetectorPackage autoResolveSmartDetectorPackage;
        private List<string> resourceIds;
        private SmartDetectorAnalysisRequest analysisRequest;
        private ContractsAlertResolutionCheckRequest alertResolutionCheckRequest;
        private TestSmartDetector smartDetector;
        private TestAutoResolveSmartDetector autoResolveSmartDetector;
        private IUnityContainer testContainer;

        private Dictionary<string, object> stateRepository;
        private Mock<IStateRepository> stateRepositoryMock;
        private Mock<IStateRepositoryFactory> stateRepositoryFactoryMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.TestInitialize(ResourceType.VirtualMachine, ResourceType.VirtualMachine);
        }

        #region Analyze tests

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeThenTheCorrectAlertIsReturned()
        {
            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
            Assert.IsNotNull(contractsAlerts, "Presentation list is null");
            Assert.AreEqual(1, contractsAlerts.Count);
            Assert.AreEqual("Test title", contractsAlerts.Single().Title);
            Assert.IsNull(contractsAlerts.Single().ResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(1, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test key"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeItIsDisposedIfItImplementsIDisposable()
        {
            this.smartDetector = new DisposableTestSmartDetector { ExpectedResourceType = ResourceType.VirtualMachine };

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));

            Assert.IsTrue(((DisposableTestSmartDetector)this.smartDetector).WasDisposed);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeWithResolutionForSupportingDetectorThenTheCorrectAlertIsReturned()
        {
            this.autoResolveSmartDetector.ShouldAutoResolve = true;
            this.analysisRequest.SmartDetectorId = "2";

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
            Assert.IsNotNull(contractsAlerts, "Presentation list is null");
            Assert.AreEqual(1, contractsAlerts.Count);
            Assert.AreEqual("Test title", contractsAlerts.Single().Title);
            Assert.IsNotNull(contractsAlerts.Single().ResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(2, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test key"]);
            Assert.IsInstanceOfType(this.stateRepository[$"_autoResolve{contractsAlerts.Single().Id}"], typeof(ResolutionState));
            var resolutionState = (ResolutionState)this.stateRepository[$"_autoResolve{contractsAlerts.Single().Id}"];
            Assert.AreEqual(1, resolutionState.AlertPredicates.Count);
            Assert.AreEqual("Predicate value", resolutionState.AlertPredicates["Predicate"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeThenCancellationIsHandledGracefully()
        {
            // Notify the Smart Detector that it should get stuck and wait for cancellation
            this.smartDetector.ShouldStuck = true;

            // Run the Smart Detector asynchronously
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.AnalyzeAsync(this.analysisRequest, true, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.smartDetector.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            FailedToRunSmartDetectorException ex = await Assert.ThrowsExceptionAsync<FailedToRunSmartDetectorException>(() => t);
            Assert.IsNull(ex.InnerException, "e.InnerException != null");
            Assert.IsTrue(ex.Message.Contains(typeof(TaskCanceledException).Name), "e.Message.Contains(typeof(TaskCanceledException).Name)");
            Assert.IsTrue(this.smartDetector.WasCanceled, "The Smart Detector was not canceled!");
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToRunSmartDetectorException))]
        public async Task WhenRunningSmartDetectorAnalyzeThenExceptionsAreHandledCorrectly()
        {
            // Notify the Smart Detector that it should throw an exception
            this.smartDetector.ShouldThrow = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
            }
            catch (FailedToRunSmartDetectorException e)
            {
                // Expected exception
                Assert.IsNull(e.InnerException, "e.InnerException != null");
                Assert.IsTrue(e.Message.Contains(typeof(DivideByZeroException).Name), "e.Message.Contains(typeof(DivideByZeroException).Name)");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToRunSmartDetectorException))]
        public async Task WhenRunningSmartDetectorAnalyzeThenCustomExceptionsAreHandledCorrectly()
        {
            // Notify the Smart Detector that it should throw a custom exception
            this.smartDetector.ShouldThrowCustom = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeWithSupportedTypeThenTheCorrectResultsAreReturned()
        {
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.Subscription, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.ResourceGroup, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.VirtualMachine, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.ResourceGroup, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.VirtualMachine, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.VirtualMachine, false);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeWithUnsupportedTypeThenAnExceptionIsThrown()
        {
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.Subscription, true);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.Subscription, true);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.ResourceGroup, true);
        }

        #endregion

        #region CheckResolution tests

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckResolutionAndAlertIsResolvedThenTheCorrectResponseIsReturned()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Setup the detector to resolve the alert
            this.autoResolveSmartDetector.ShouldResolve = true;

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            ContractsAlertResolutionCheckResponse alertResolutionCheckResponse =
                await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));

            Assert.IsTrue(alertResolutionCheckResponse.ShouldBeResolved);
            Assert.IsNull(alertResolutionCheckResponse.ResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(1, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test auto resolve key"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckResolutionItIsDisposedIfItImplementsIDisposable()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Make the detector  a disposable one
            this.autoResolveSmartDetector = new DisposableTestAutoResolveSmartDetector { ExpectedResourceType = ResourceType.VirtualMachine };

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));

            Assert.IsTrue(((DisposableTestAutoResolveSmartDetector)this.autoResolveSmartDetector).WasDisposed);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckResolutionAndAlertIsNotResolvedThenTheCorrectResponseIsReturned()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Setup the detector to not resolve the alert
            this.autoResolveSmartDetector.ShouldResolve = false;

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            ContractsAlertResolutionCheckResponse alertResolutionCheckResponse =
                await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));

            Assert.IsFalse(alertResolutionCheckResponse.ShouldBeResolved);
            Assert.IsNotNull(alertResolutionCheckResponse.ResolutionParameters);
            Assert.AreEqual(TimeSpan.FromMinutes(15), alertResolutionCheckResponse.ResolutionParameters.CheckForResolutionAfter);

            // Assert the detector's state
            Assert.AreEqual(2, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test auto resolve key"]);
            Assert.IsTrue(this.stateRepository.ContainsKey($"_autoResolve{this.alertResolutionCheckRequest.AlertCorrelationHash}"));
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckResolutionThenCancellationIsHandledGracefully()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Notify the Smart Detector that it should get stuck and wait for cancellation
            this.autoResolveSmartDetector.ShouldStuck = true;

            // Run the Smart Detector asynchronously
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.autoResolveSmartDetector.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            FailedToRunSmartDetectorException ex = await Assert.ThrowsExceptionAsync<FailedToRunSmartDetectorException>(() => t);
            Assert.IsNull(ex.InnerException, "e.InnerException != null");
            Assert.IsTrue(ex.Message.Contains(typeof(TaskCanceledException).Name), "e.Message.Contains(typeof(TaskCanceledException).Name)");
            Assert.IsTrue(this.autoResolveSmartDetector.WasCanceled, "The Smart Detector was not canceled!");
        }

        [TestMethod]
        [ExpectedException(typeof(ResolutionCheckNotSupportedException))]
        public async Task WhenRunningSmartDetectorCheckResolutionForNonSupportingDetectorThenExceptionIsThrown()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Set the detector to be non supporting
            this.alertResolutionCheckRequest.OriginalAnalysisRequest.SmartDetectorId = "1";

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(ResolutionStateNotFoundException))]
        public async Task WhenRunningSmartDetectorCheckResolutionAndStateIsNotFoundThenExceptionIsThrown()
        {
            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToRunSmartDetectorException))]
        public async Task WhenRunningSmartDetectorCheckResolutionThenExceptionsAreHandledCorrectly()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Notify the Smart Detector that it should throw an exception
            this.autoResolveSmartDetector.ShouldThrow = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));
            }
            catch (FailedToRunSmartDetectorException e)
            {
                // Expected exception
                Assert.IsNull(e.InnerException, "e.InnerException != null");
                Assert.IsTrue(e.Message.Contains(typeof(DivideByZeroException).Name), "e.Message.Contains(typeof(DivideByZeroException).Name)");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToRunSmartDetectorException))]
        public async Task WhenRunningSmartDetectorCheckResolutionThenCustomExceptionsAreHandledCorrectly()
        {
            // Initialize the resolution state
            this.InitializeResolutionState();

            // Notify the Smart Detector that it should throw a custom exception
            this.autoResolveSmartDetector.ShouldThrowCustom = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckResolutionAsync(this.alertResolutionCheckRequest, true, default(CancellationToken));
        }

        #endregion

        private async Task RunSmartDetectorWithResourceTypes(ResourceType requestResourceType, ResourceType smartDetectorResourceType, bool shouldFail)
        {
            this.TestInitialize(requestResourceType, smartDetectorResourceType);
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                List<ContractsAlert> alertPresentations = await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
                if (shouldFail)
                {
                    Assert.Fail("An exception should have been thrown - resource types are not compatible");
                }

                Assert.AreEqual(1, alertPresentations.Count);
            }
            catch (IncompatibleResourceTypesException)
            {
                if (!shouldFail)
                {
                    throw;
                }
            }
        }

        private void TestInitialize(ResourceType requestResourceType, ResourceType smartDetectorResourceType)
        {
            this.testContainer = new UnityContainer();

            this.testContainer.RegisterType<ISmartDetectorRunner, SmartDetectorRunner>();

            this.testContainer.RegisterInstance(new Mock<IExtendedTracer>().Object);

            ResourceIdentifier resourceId;
            switch (requestResourceType)
            {
                case ResourceType.Subscription:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", string.Empty, string.Empty);
                    break;
                case ResourceType.ResourceGroup:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", "resourceGroup", string.Empty);
                    break;
                default:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", "resourceGroup", "resourceName");
                    break;
            }

            this.resourceIds = new List<string> { resourceId.ToResourceId() };
            this.analysisRequest = new SmartDetectorAnalysisRequest
            {
                ResourceIds = this.resourceIds,
                Cadence = TimeSpan.FromDays(1),
                AlertRuleResourceId = "alertRule",
                SmartDetectorId = "1",
                DetectorParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" },
                    { "param2", 2 },
                }
            };
            this.alertResolutionCheckRequest = new ContractsAlertResolutionCheckRequest
            {
                OriginalAnalysisRequest = new SmartDetectorAnalysisRequest
                {
                    ResourceIds = this.resourceIds,
                    Cadence = TimeSpan.FromDays(1),
                    AlertRuleResourceId = "alertRule",
                    SmartDetectorId = "2",
                    DetectorParameters = new Dictionary<string, object>
                    {
                        { "param1", "value1" },
                        { "param2", 2 },
                    }
                },
                AlertCorrelationHash = "correlationHash",
                TargetResource = resourceId.ToResourceId(),
                AlertFireTime = new DateTime(1985, 7, 3)
            };

            var smartDetectorManifest = new SmartDetectorManifest(
                "1",
                "Test Smart Detector",
                "Test Smart Detector description",
                Version.Parse("1.0"),
                "TestSmartDetectorLibrary",
                "class",
                new List<ResourceType>() { smartDetectorResourceType },
                new List<int> { 60 },
                null,
                null);
            this.smartDetectorPackage = new SmartDetectorPackage(new Dictionary<string, byte[]>
            {
                ["manifest.json"] = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(smartDetectorManifest)),
                ["TestSmartDetectorLibrary"] = Array.Empty<byte>(),
            });

            var autoResolveSmartDetectorManifest = new SmartDetectorManifest(
                "2",
                "Test Auto Resolve Smart Detector",
                "Test Auto Resolve Smart Detector description",
                Version.Parse("1.0"),
                "TestSmartDetectorLibrary",
                "class",
                new List<ResourceType>() { smartDetectorResourceType },
                new List<int> { 60 },
                null,
                null);
            this.autoResolveSmartDetectorPackage = new SmartDetectorPackage(new Dictionary<string, byte[]>
            {
                ["manifest.json"] = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(autoResolveSmartDetectorManifest)),
                ["TestSmartDetectorLibrary"] = Array.Empty<byte>(),
            });

            var smartDetectorRepositoryMock = new Mock<ISmartDetectorRepository>();
            smartDetectorRepositoryMock
                .Setup(x => x.ReadSmartDetectorPackageAsync("1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => this.smartDetectorPackage);
            smartDetectorRepositoryMock
                .Setup(x => x.ReadSmartDetectorPackageAsync("2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => this.autoResolveSmartDetectorPackage);
            this.testContainer.RegisterInstance(smartDetectorRepositoryMock.Object);

            this.testContainer.RegisterInstance(new Mock<IInternalAnalysisServicesFactory>().Object);

            this.smartDetector = new TestSmartDetector { ExpectedResourceType = smartDetectorResourceType };
            this.autoResolveSmartDetector = new TestAutoResolveSmartDetector { ExpectedResourceType = smartDetectorResourceType };

            var smartDetectorLoaderMock = new Mock<ISmartDetectorLoader>();
            smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.smartDetectorPackage))
                .Returns(() => this.smartDetector);
            smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.autoResolveSmartDetectorPackage))
                .Returns(() => this.autoResolveSmartDetector);
            this.testContainer.RegisterInstance(smartDetectorLoaderMock.Object);

            var azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourceGroupsInSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.ResourceGroup, subscriptionId, "resourceGroupName", string.Empty) });
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, "resourceGroupName", "resourceName") });
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInResourceGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, resourceGroupName, "resourceName") });
            this.testContainer.RegisterInstance(azureResourceManagerClientMock.Object);

            this.testContainer.RegisterInstance(new Mock<IQueryRunInfoProvider>().Object);

            this.stateRepository = new Dictionary<string, object>();
            this.stateRepositoryMock = new Mock<IStateRepository>();
            this.stateRepositoryMock
                .Setup(m => m.StoreStateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback<string, object, CancellationToken>((key, value, token) => this.stateRepository[key] = value)
                .Returns(Task.CompletedTask);
            this.stateRepositoryMock
                .Setup(m => m.GetStateAsync<ResolutionState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((key, token) => Task.FromResult((ResolutionState)(this.stateRepository.ContainsKey(key) ? this.stateRepository[key] : null)));
            this.stateRepositoryMock
                .Setup(m => m.DeleteStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((key, token) => this.stateRepository.Remove(key))
                .Returns(Task.CompletedTask);
            this.stateRepositoryFactoryMock = new Mock<IStateRepositoryFactory>();
            this.stateRepositoryFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(this.stateRepositoryMock.Object);
            this.testContainer.RegisterInstance(this.stateRepositoryFactoryMock.Object);
        }

        private void InitializeResolutionState()
        {
            var state = new ResolutionState
            {
                AlertPredicates = new Dictionary<string, object>
                {
                    ["Predicate"] = "Predicate value"
                }
            };

            this.stateRepository[$"_autoResolve{this.alertResolutionCheckRequest.AlertCorrelationHash}"] = state;
            this.autoResolveSmartDetector.ShouldResolve = true;
        }

        public class TestSmartDetector : ISmartDetector
        {
            public bool ShouldStuck { get; set; }

            public bool ShouldThrow { get; set; }

            public bool ShouldThrowCustom { get; set; }

            public bool ShouldAutoResolve { get; set; }

            public bool IsRunning { get; protected set; }

            public bool WasCanceled { get; protected set; }

            public ResourceType ExpectedResourceType { get; set; }

            public async Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                this.AssertAnalysisRequestParameters(analysisRequest.RequestParameters);

                await analysisRequest.StateRepository.StoreStateAsync("test key", "test state", cancellationToken);

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

                if (this.ShouldThrow)
                {
                    throw new DivideByZeroException();
                }

                if (this.ShouldThrowCustom)
                {
                    throw new CustomException();
                }

                return new List<Alert>
                {
                    new TestAlert(analysisRequest.RequestParameters.TargetResources.First(), this.ShouldAutoResolve)
                };
            }

            protected void AssertAnalysisRequestParameters(AnalysisRequestParameters analysisRequestParameters)
            {
                Assert.IsNotNull(analysisRequestParameters.TargetResources, "Resources list is null");
                Assert.AreEqual(1, analysisRequestParameters.TargetResources.Count);
                Assert.AreEqual(this.ExpectedResourceType, analysisRequestParameters.TargetResources.Single().ResourceType);

                Assert.AreEqual("alertRule", analysisRequestParameters.AlertRuleResourceId);

                Assert.AreEqual(TimeSpan.FromDays(1), analysisRequestParameters.AnalysisCadence);

                Assert.AreEqual(2, analysisRequestParameters.DetectorParameters.Count);
                Assert.AreEqual("value1", analysisRequestParameters.DetectorParameters["param1"]);
                Assert.AreEqual(2, analysisRequestParameters.DetectorParameters["param2"]);
            }

            [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Test class, allowed")]
            [SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Test class, allowed")]
            public class CustomException : Exception
            {
            }
        }

        public class TestAutoResolveSmartDetector : TestSmartDetector, IAlertResolutionSmartDetector
        {
            public bool ShouldResolve { get; set; } = true;

            #region Implementation of IAlertResolutionSmartDetector

            public async Task<AlertResolutionCheckResponse> CheckForResolutionAsync(
                AlertResolutionCheckRequest alertResolutionCheckRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                this.AssertAnalysisRequestParameters(alertResolutionCheckRequest.OriginalAnalysisRequestParameters);
                Assert.AreEqual(alertResolutionCheckRequest.OriginalAnalysisRequestParameters.TargetResources.Single(), alertResolutionCheckRequest.RequestParameters.ResourceIdentifier);
                Assert.AreEqual(new DateTime(1985, 7, 3), alertResolutionCheckRequest.RequestParameters.AlertFireTime);
                Assert.AreEqual(1, alertResolutionCheckRequest.RequestParameters.AlertPredicates.Count);
                Assert.AreEqual("Predicate", alertResolutionCheckRequest.RequestParameters.AlertPredicates.Single().Key);
                Assert.AreEqual("Predicate value", alertResolutionCheckRequest.RequestParameters.AlertPredicates.Single().Value);

                await alertResolutionCheckRequest.StateRepository.StoreStateAsync("test auto resolve key", "test state", cancellationToken);

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

                if (this.ShouldThrow)
                {
                    throw new DivideByZeroException();
                }

                if (this.ShouldThrowCustom)
                {
                    throw new CustomException();
                }

                return this.ShouldResolve
                    ? new AlertResolutionCheckResponse(true, null)
                    : new AlertResolutionCheckResponse(false, new AlertResolutionParameters { CheckForResolutionAfter = TimeSpan.FromMinutes(15) });
            }

            #endregion
        }

        public class TestAlert : Alert
        {
            public TestAlert(ResourceIdentifier resourceIdentifier, bool shouldAutoResolve)
                : base("Test title", resourceIdentifier)
            {
                if (shouldAutoResolve)
                {
                    this.AlertResolutionParameters = new AlertResolutionParameters
                    {
                        CheckForResolutionAfter = TimeSpan.FromMinutes(5)
                    };
                }
            }

            [AlertPresentationProperty(AlertPresentationSection.Property, "Summary title", InfoBalloon = "Summary info")]
            public string Summary { get; } = "Summary value";

            [PredicateProperty]
            public string Predicate { get; } = "Predicate value";
        }

        public sealed class DisposableTestSmartDetector : TestSmartDetector, IDisposable
        {
            public DisposableTestSmartDetector()
            {
                this.WasDisposed = false;
            }

            public bool WasDisposed { get; private set; }

            public void Dispose()
            {
                this.WasDisposed = true;
            }
        }

        public sealed class DisposableTestAutoResolveSmartDetector : TestAutoResolveSmartDetector, IDisposable
        {
            public DisposableTestAutoResolveSmartDetector()
            {
                this.WasDisposed = false;
            }

            public bool WasDisposed { get; private set; }

            public void Dispose()
            {
                this.WasDisposed = true;
            }
        }
    }
}