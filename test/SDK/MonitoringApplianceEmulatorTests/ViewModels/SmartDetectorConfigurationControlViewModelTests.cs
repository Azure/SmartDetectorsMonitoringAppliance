//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfigurationControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartDetectorConfigurationControlViewModelTests
    {
        private Mock<IEmulationSmartDetectorRunner> smartDetectorRunnerMock;
        private Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock;
        private Mock<ITracer> tracerMock;

        private UserSettings userSettings;
        private SmartDetectorManifest smartDetectorManifest;

        private SmartDetectorConfigurationControlViewModel smartDetectorConfigurationControlViewModel;

        [TestInitialize]
        public void Setup()
        {
            this.smartDetectorRunnerMock = new Mock<IEmulationSmartDetectorRunner>();
            this.azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();
            this.tracerMock = new Mock<ITracer>();

            this.userSettings = new UserSettings();

            this.smartDetectorManifest = new SmartDetectorManifest(
                "someId",
                "someName",
                "someDisplayName",
                Version.Parse("1.0"),
                "someAssemblyName",
                "someClassName",
                new List<ResourceType> { ResourceType.ResourceGroup, ResourceType.VirtualMachine },
                new List<int> { 10, 60, 120 },
                null,
                null);

            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllSubscriptionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<AzureSubscription>() { new AzureSubscription("subId1", "subDisplayName1") });

            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, "subId1", "someResourceGroup", "someVM") });

            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourceGroupsInSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.ResourceGroup, "subId1", "someResourceGroup", string.Empty) });

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            List<SmartDetectorCadence> expectedCadences = this.smartDetectorManifest.SupportedCadencesInMinutes
                .Select(cadence => new SmartDetectorCadence(TimeSpan.FromMinutes(cadence))).ToList();

            Assert.AreEqual("someName", this.smartDetectorConfigurationControlViewModel.SmartDetectorName);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldShowStatusControl);
            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.IterativeRunModeEnabled);

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.Cadences);
            Assert.AreEqual(3, this.smartDetectorConfigurationControlViewModel.Cadences.Count);
            for (int i = 0; i < this.smartDetectorConfigurationControlViewModel.Cadences.Count; i++)
            {
                VerifyCadence(expectedCadences[i], this.smartDetectorConfigurationControlViewModel.Cadences[i]);
            }

            VerifyCadence(expectedCadences[0], this.smartDetectorConfigurationControlViewModel.SelectedCadence);

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.SupportedResourceTypes);
            Assert.AreEqual(3, this.smartDetectorConfigurationControlViewModel.SupportedResourceTypes.Count);
            Assert.AreEqual("All", this.smartDetectorConfigurationControlViewModel.SupportedResourceTypes[0]);
            Assert.AreEqual(ResourceType.ResourceGroup.ToString(), this.smartDetectorConfigurationControlViewModel.SupportedResourceTypes[1]);
            Assert.AreEqual(ResourceType.VirtualMachine.ToString(), this.smartDetectorConfigurationControlViewModel.SupportedResourceTypes[2]);

            Assert.AreEqual("All", this.smartDetectorConfigurationControlViewModel.SelectedResourceType);

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ReadSubscriptionsTask);
            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ReadResourcesTask);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeAndUserSettingsExistThenResourcesWereLoadedFromUserSettings()
        {
            var expectedSelectedSubscription = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            var expectedSelectedResource = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.VirtualMachine, "subId1", "someResourceGroup", "someVM"),
                new List<HierarchicalResource>(),
                "someVM");

            this.userSettings.SelectedSubscription = "subDisplayName1";
            this.userSettings.SelectedResourceType = ResourceType.VirtualMachine.ToString();
            this.userSettings.SelectedResource = "someVM";

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            // Wait for subscriptions & resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            VerifyHierarchicalResource(expectedSelectedSubscription, this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
            Assert.AreEqual(ResourceType.VirtualMachine.ToString(), this.smartDetectorConfigurationControlViewModel.SelectedResourceType);
            VerifyHierarchicalResource(expectedSelectedResource, this.smartDetectorConfigurationControlViewModel.SelectedResource);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldSelectResourcesAccordingToUserSettings);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeAndUserSettingsExistWithoutSubscriptionThenNoResourcesWereLoadedFromUserSettings()
        {
            this.userSettings.SelectedSubscription = null;
            this.userSettings.SelectedResourceType = ResourceType.VirtualMachine.ToString();
            this.userSettings.SelectedResource = "someVM";

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            // Wait for subscriptions & resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
            Assert.AreEqual("All", this.smartDetectorConfigurationControlViewModel.SelectedResourceType);
            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedResource);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldSelectResourcesAccordingToUserSettings);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeAndUserSettingsExistButSubscriptionNoLongetExistsThenNoResourcesWereLoadedFromUserSettings()
        {
            this.userSettings.SelectedSubscription = "deletedSubDisplayName";
            this.userSettings.SelectedResourceType = ResourceType.VirtualMachine.ToString();
            this.userSettings.SelectedResource = "someVM";

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            // Wait for subscriptions & resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
            Assert.AreEqual(ResourceType.VirtualMachine.ToString(), this.smartDetectorConfigurationControlViewModel.SelectedResourceType);
            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedResource);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldSelectResourcesAccordingToUserSettings);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeAndUserSettingsExistButResourceTypeIsNoLongerSupportedThenNoResourceTypeAndNoResourceWereLoadedFromUserSettings()
        {
            var expectedSelectedSubscription = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            this.userSettings.SelectedSubscription = "subDisplayName1";
            this.userSettings.SelectedResourceType = "NonExistingResourceType";
            this.userSettings.SelectedResource = "someVM";

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            // Wait for subscriptions & resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            VerifyHierarchicalResource(expectedSelectedSubscription, this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
            Assert.AreEqual("All", this.smartDetectorConfigurationControlViewModel.SelectedResourceType);
            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedResource);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldSelectResourcesAccordingToUserSettings);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeAndUserSettingsExistButResourceNoLongerExistsThenNoResourceIsLoadedFromUserSettings()
        {
            var expectedSelectedSubscription = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            this.userSettings.SelectedSubscription = "subDisplayName1";
            this.userSettings.SelectedResourceType = ResourceType.VirtualMachine.ToString();
            this.userSettings.SelectedResource = "someDeletedVM";

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            // Wait for subscriptions & resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            VerifyHierarchicalResource(expectedSelectedSubscription, this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
            Assert.AreEqual(ResourceType.VirtualMachine.ToString(), this.smartDetectorConfigurationControlViewModel.SelectedResourceType);
            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedResource);

            Assert.IsFalse(this.smartDetectorConfigurationControlViewModel.ShouldSelectResourcesAccordingToUserSettings);
        }

        [TestMethod]
        public async Task WhenCreatingNewViewModeThenReadSubscriptionsTaskIsFired()
        {
            // Wait for subscriptions task
            await Task.Delay(TimeSpan.FromSeconds(1));

            var expectedSubscription = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ReadSubscriptionsTask.Result);
            Assert.AreEqual(1, this.smartDetectorConfigurationControlViewModel.ReadSubscriptionsTask.Result.Count);
            VerifyHierarchicalResource(expectedSubscription, this.smartDetectorConfigurationControlViewModel.ReadSubscriptionsTask.Result[0]);

            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.SelectedSubscription);
        }

        [TestMethod]
        public async Task WhenSelectingSubscriptionThenTheReadResourcesTaskIsFired()
        {
            var subscriptionToSelect = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            this.smartDetectorConfigurationControlViewModel.SelectedSubscription = subscriptionToSelect;

            // Wait for resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            var expectedFirstResourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, "subId1", "someResourceGroup", "someVM");
            var expectedSecondResourceIdentifier = new ResourceIdentifier(ResourceType.ResourceGroup, "subId1", "someResourceGroup", string.Empty);

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result);
            Assert.AreEqual(2, this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result.Count);

            Assert.AreEqual(expectedFirstResourceIdentifier, this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result[0]);
            Assert.AreEqual(expectedSecondResourceIdentifier, this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result[1]);
        }

        [TestMethod]
        public async Task WhenSelectingSubscriptionForDetectorThatSupportsOnlyResourceGroupsThenTheReadResourcesTaskIsFiredAndFetchingOnlyResourceGroups()
        {
            // Update detector manifest to support only resource groups
            this.smartDetectorManifest = new SmartDetectorManifest(
                "someId",
                "someName",
                "someDescription",
                Version.Parse("1.0"),
                "someAssemblyName",
                "someClassName",
                new List<ResourceType> { ResourceType.ResourceGroup },
                new List<int> { 10, 60, 120 },
                null,
                null);

            this.smartDetectorConfigurationControlViewModel = new SmartDetectorConfigurationControlViewModel(
                this.azureResourceManagerClientMock.Object,
                this.tracerMock.Object,
                this.smartDetectorManifest,
                this.smartDetectorRunnerMock.Object,
                this.userSettings);

            var subscriptionToSelect = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            this.smartDetectorConfigurationControlViewModel.SelectedSubscription = subscriptionToSelect;

            // Wait for resources task
            await Task.Delay(TimeSpan.FromSeconds(1));

            var expectedFirstResourceIdentifier = new ResourceIdentifier(ResourceType.ResourceGroup, "subId1", "someResourceGroup", string.Empty);

            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result);
            Assert.AreEqual(1, this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result.Count);

            Assert.AreEqual(expectedFirstResourceIdentifier, this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result[0]);
        }

        [TestMethod]
        public async Task WhenSelectingResourceTypeThenHierarchicalResourcesCollectionWasFiltered()
        {
            // Wait for subscriptions task
            await Task.Delay(TimeSpan.FromSeconds(1));

            var subscriptionToSelect = new HierarchicalResource(
                new ResourceIdentifier(ResourceType.Subscription, "subId1", string.Empty, string.Empty),
                new List<HierarchicalResource>(),
                "subDisplayName1");

            this.smartDetectorConfigurationControlViewModel.SelectedSubscription = subscriptionToSelect;

            // Wait for resources task to finish and then to resources hierarchical collection to be created
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Make sure VM resource exist before filtering
            Assert.IsNotNull(this.smartDetectorConfigurationControlViewModel.ResourcesHierarchicalCollection.TryFind("someVM"));

            this.smartDetectorConfigurationControlViewModel.SelectedResourceType = ResourceType.ApplicationInsights.ToString();

            // Verify VM resource was not filtered
            Assert.IsNull(this.smartDetectorConfigurationControlViewModel.ResourcesHierarchicalCollection.TryFind("someVM"));
        }

        [TestMethod]
        public void WhenExecutingCancelDetectorRunCommandThenTheDetectorRunWasCanceled()
        {
            this.smartDetectorConfigurationControlViewModel.CancelSmartDetectorRunCommand.Execute(parameter: null);

            this.smartDetectorRunnerMock.Verify(m => m.CancelSmartDetectorRun(), Times.Once());
        }

        [TestMethod]
        public void WhenExecutingRunDetectorCommandWithIterativeModeDisabledThenTheDetectorIsRunningWithExpectedParameters()
        {
            this.smartDetectorConfigurationControlViewModel.RunSmartDetectorCommand.Execute(parameter: null);

            Assert.IsTrue(this.smartDetectorConfigurationControlViewModel.ShouldShowStatusControl);

            this.smartDetectorRunnerMock.Verify(
                m => m.RunAsync(
                    this.smartDetectorConfigurationControlViewModel.SelectedResource,
                    this.smartDetectorConfigurationControlViewModel.ReadResourcesTask.Result,
                    this.smartDetectorConfigurationControlViewModel.SelectedCadence.TimeSpan,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()),
                Times.Once());
        }

        private static void VerifyCadence(SmartDetectorCadence expected, SmartDetectorCadence actual)
        {
            Assert.AreEqual(expected.DisplayName, actual.DisplayName);
            Assert.AreEqual(expected.TimeSpan, actual.TimeSpan);
        }

        private static void VerifyHierarchicalResource(HierarchicalResource expected, HierarchicalResource actual)
        {
            Assert.AreEqual(expected.ResourceIdentifier, actual.ResourceIdentifier);
            Assert.AreEqual(expected.Name, actual.Name);
            CollectionAssert.AreEqual(expected.ContainedResources.OriginalCollection, actual.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(expected.ContainedResources.FilteredCollection, actual.ContainedResources.FilteredCollection);
        }
    }
}
