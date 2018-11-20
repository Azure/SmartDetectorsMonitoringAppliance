﻿//-----------------------------------------------------------------------
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
    using AutomaticResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.AutomaticResolutionCheckRequest;
    using AutomaticResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.AutomaticResolutionCheckResponse;
    using AutomaticResolutionParameters = Microsoft.Azure.Monitoring.SmartDetectors.AutomaticResolutionParameters;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsAutomaticResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AutomaticResolutionCheckRequest;
    using ContractsAutomaticResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AutomaticResolutionCheckResponse;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    [SuppressMessage("Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test class - allowed")]
    public class SmartDetectorRunnerTests
    {
        private SmartDetectorPackage smartDetectorPackage;
        private SmartDetectorPackage autoResolveSmartDetectorPackage;
        private List<string> resourceIds;
        private SmartDetectorAnalysisRequest analysisRequest;
        private ContractsAutomaticResolutionCheckRequest automaticResolutionCheckRequest;
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
            Assert.IsNull(contractsAlerts.Single().AutomaticResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(1, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test key"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorItIsDisposedIfItImplementsIDisposable()
        {
            this.smartDetector = new DisposableTestSmartDetector { ExpectedResourceType = ResourceType.VirtualMachine };

            var smartDetectorLoaderMock = new Mock<ISmartDetectorLoader>();
            smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.smartDetectorPackage))
                .Returns(this.smartDetector);
            this.testContainer.RegisterInstance<ISmartDetectorLoader>(smartDetectorLoaderMock.Object);

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.RunAsync(this.request, true, default(CancellationToken));

            Assert.IsTrue(((DisposableTestSmartDetector)this.smartDetector).WasDisposed);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void WhenRunningSmartDetectorThenCancellationIsHandledGracefully()
        {
            this.smartDetector.ShouldAutoResolve = true;

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
            Assert.IsNotNull(contractsAlerts, "Presentation list is null");
            Assert.AreEqual(1, contractsAlerts.Count);
            Assert.AreEqual("Test title", contractsAlerts.Single().Title);
            Assert.IsNull(contractsAlerts.Single().AutomaticResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(1, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test key"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorAnalyzeWithAutomaticResolutionForSupportingDetectorThenTheCorrectAlertIsReturned()
        {
            this.autoResolveSmartDetector.ShouldAutoResolve = true;
            this.analysisRequest.SmartDetectorId = "2";

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.AnalyzeAsync(this.analysisRequest, true, default(CancellationToken));
            Assert.IsNotNull(contractsAlerts, "Presentation list is null");
            Assert.AreEqual(1, contractsAlerts.Count);
            Assert.AreEqual("Test title", contractsAlerts.Single().Title);
            Assert.IsNotNull(contractsAlerts.Single().AutomaticResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(2, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test key"]);
            Assert.IsInstanceOfType(this.stateRepository[$"_autoResolve{contractsAlerts.Single().Id}"], typeof(AutomaticResolutionState));
            var automaticResolutionState = (AutomaticResolutionState)this.stateRepository[$"_autoResolve{contractsAlerts.Single().Id}"];
            Assert.AreEqual(contractsAlerts.Single().Id, automaticResolutionState.AlertId);
            Assert.AreEqual(1, automaticResolutionState.AlertPredicates.Count);
            Assert.AreEqual("Predicate value", automaticResolutionState.AlertPredicates["Predicate"]);
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

        #region CheckAutomaticResolution tests

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionAndAlertIsResolvedThenTheCorrectResponseIsReturned()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Setup the detector to resolve the alert
            this.autoResolveSmartDetector.ShouldResolve = true;

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            ContractsAutomaticResolutionCheckResponse automaticResolutionCheckResponse =
                await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));

            Assert.IsTrue(automaticResolutionCheckResponse.ShouldBeResolved);
            Assert.IsNull(automaticResolutionCheckResponse.AutomaticResolutionParameters);

            // Assert the detector's state
            Assert.AreEqual(1, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test auto resolve key"]);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionAndAlertIsNotResolvedThenTheCorrectResponseIsReturned()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Setup the detector to not resolve the alert
            this.autoResolveSmartDetector.ShouldResolve = false;

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            ContractsAutomaticResolutionCheckResponse automaticResolutionCheckResponse =
                await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));

            Assert.IsFalse(automaticResolutionCheckResponse.ShouldBeResolved);
            Assert.IsNotNull(automaticResolutionCheckResponse.AutomaticResolutionParameters);
            Assert.AreEqual(TimeSpan.FromMinutes(15), automaticResolutionCheckResponse.AutomaticResolutionParameters.CheckForAutomaticResolutionAfter);

            // Assert the detector's state
            Assert.AreEqual(2, this.stateRepository.Count);
            Assert.AreEqual("test state", this.stateRepository["test auto resolve key"]);
            Assert.IsTrue(this.stateRepository.ContainsKey($"_autoResolve{this.automaticResolutionCheckRequest.AlertId}"));
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionThenCancellationIsHandledGracefully()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Notify the Smart Detector that it should get stuck and wait for cancellation
            this.autoResolveSmartDetector.ShouldStuck = true;

            // Run the Smart Detector asynchronously
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.autoResolveSmartDetector.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            FailedToRunSmartDetectorException ex = await Assert.ThrowsExceptionAsync<FailedToRunSmartDetectorException>(() => t);
            Assert.IsNull(ex.InnerException, "e.InnerException != null");
            Assert.IsTrue(ex.Message.Contains(typeof(TaskCanceledException).Name), "e.Message.Contains(typeof(TaskCanceledException).Name)");
            Assert.IsTrue(this.autoResolveSmartDetector.WasCanceled, "The Smart Detector was not canceled!");
        }

        [TestMethod]
        [ExpectedException(typeof(AutomaticResolutionNotSupportedException))]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionForNonSupportingDetectorThenExceptionIsThrown()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Set the detector to be non supporting
            this.automaticResolutionCheckRequest.OriginalAnalysisRequest.SmartDetectorId = "1";

            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(AutomaticResolutionStateNotFoundException))]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionAndStateIsNotFoundThenExceptionIsThrown()
        {
            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToRunSmartDetectorException))]
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionThenExceptionsAreHandledCorrectly()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Notify the Smart Detector that it should throw an exception
            this.autoResolveSmartDetector.ShouldThrow = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));
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
        public async Task WhenRunningSmartDetectorCheckAutomaticResolutionThenCustomExceptionsAreHandledCorrectly()
        {
            // Initialize the automatic resolution state
            this.InitializeAutomaticResolutionState();

            // Notify the Smart Detector that it should throw a custom exception
            this.autoResolveSmartDetector.ShouldThrowCustom = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.CheckAutomaticResolutionAsync(this.automaticResolutionCheckRequest, true, default(CancellationToken));
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
            this.automaticResolutionCheckRequest = new ContractsAutomaticResolutionCheckRequest
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
                AlertId = "alertId",
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
                .Returns(this.smartDetector);
            smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.autoResolveSmartDetectorPackage))
                .Returns(this.autoResolveSmartDetector);
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
                .Setup(m => m.GetStateAsync<AutomaticResolutionState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((key, token) => Task.FromResult((AutomaticResolutionState)(this.stateRepository.ContainsKey(key) ? this.stateRepository[key] : null)));
            this.stateRepositoryMock
                .Setup(m => m.DeleteStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((key, token) => this.stateRepository.Remove(key))
                .Returns(Task.CompletedTask);
            this.stateRepositoryFactoryMock = new Mock<IStateRepositoryFactory>();
            this.stateRepositoryFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(this.stateRepositoryMock.Object);
            this.testContainer.RegisterInstance(this.stateRepositoryFactoryMock.Object);
        }

        private void InitializeAutomaticResolutionState()
        {
            var state = new AutomaticResolutionState
            {
                AlertId = this.automaticResolutionCheckRequest.AlertId,
                AlertPredicates = new Dictionary<string, object>
                {
                    ["Predicate"] = "Predicate value"
                }
            };

            this.stateRepository[$"_autoResolve{this.automaticResolutionCheckRequest.AlertId}"] = state;
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

        public class TestAutoResolveSmartDetector : TestSmartDetector, IAutomaticResolutionSmartDetector
        {
            public bool ShouldResolve { get; set; } = true;

            #region Implementation of IAutomaticResolutionSmartDetector

            public async Task<AutomaticResolutionCheckResponse> CheckForAutomaticResolutionAsync(
                AutomaticResolutionCheckRequest automaticResolutionCheckRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                this.AssertAnalysisRequestParameters(automaticResolutionCheckRequest.OriginalAnalysisRequestParameters);
                Assert.AreEqual(automaticResolutionCheckRequest.OriginalAnalysisRequestParameters.TargetResources.Single(), automaticResolutionCheckRequest.RequestParameters.ResourceIdentifier);
                Assert.AreEqual(new DateTime(1985, 7, 3), automaticResolutionCheckRequest.RequestParameters.AlertFireTime);
                Assert.AreEqual(1, automaticResolutionCheckRequest.RequestParameters.AlertPredicates.Count);
                Assert.AreEqual("Predicate", automaticResolutionCheckRequest.RequestParameters.AlertPredicates.Single().Key);
                Assert.AreEqual("Predicate value", automaticResolutionCheckRequest.RequestParameters.AlertPredicates.Single().Value);

                await automaticResolutionCheckRequest.StateRepository.StoreStateAsync("test auto resolve key", "test state", cancellationToken);

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
                    ? new AutomaticResolutionCheckResponse(true, null)
                    : new AutomaticResolutionCheckResponse(false, new AutomaticResolutionParameters { CheckForAutomaticResolutionAfter = TimeSpan.FromMinutes(15) });
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
                    this.AutomaticResolutionParameters = new AutomaticResolutionParameters
                    {
                        CheckForAutomaticResolutionAfter = TimeSpan.FromMinutes(5)
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
    }
}