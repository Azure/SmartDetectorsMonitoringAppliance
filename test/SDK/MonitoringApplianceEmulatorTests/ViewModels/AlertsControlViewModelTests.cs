//-----------------------------------------------------------------------
// <copyright file="AlertsControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertsControlViewModelTests
    {
        private SmartDetectorRunner smartDetectorRunner;

        [TestInitialize]
        public void Setup()
        {
            Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();

            this.smartDetectorRunner = new SmartDetectorRunner(
                new Mock<ISmartDetector>().Object,
                new Mock<IInternalAnalysisServicesFactory>().Object,
                new QueryRunInfoProvider(azureResourceManagerClientMock.Object),
                null,
                new Mock<IStateRepositoryFactory>().Object,
                azureResourceManagerClientMock.Object,
                new Mock<IPageableLogArchive>().Object);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            var alertsControlViewModel = new AlertsControlViewModel(this.smartDetectorRunner);

            Assert.AreEqual(this.smartDetectorRunner, alertsControlViewModel.SmartDetectorRunner, "Unexpected smart detector runner");
            Assert.IsNull(alertsControlViewModel.SelectedAlert, "Selected alert should be null");
            Assert.IsNull(alertsControlViewModel.AlertDetailsControlViewModel, "Alert details control view model should be null");
        }

        [TestMethod]
        public void WhenAlertWasSelectedThenAlertDetailsViewModelWasUpdatedAccordingly()
        {
            var alertsControlViewModel = new AlertsControlViewModel(this.smartDetectorRunner);
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert());

            alertsControlViewModel.SelectedAlert = emulationAlert;

            Assert.IsNotNull(alertsControlViewModel.AlertDetailsControlViewModel, "Alert details control view model should not be null");
            Assert.AreEqual(emulationAlert, alertsControlViewModel.AlertDetailsControlViewModel.Alert, "Unexpected alert details control view model");

            EmulationAlert anotherEmulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert());

            alertsControlViewModel.SelectedAlert = anotherEmulationAlert;

            Assert.IsNotNull(alertsControlViewModel.AlertDetailsControlViewModel, "Alert details control view model should not be null");
            Assert.AreEqual(anotherEmulationAlert, alertsControlViewModel.AlertDetailsControlViewModel.Alert, "Unexpected alert details control view model");
        }

        [TestMethod]
        public void WhenAlertDetailsControlClosedEventWasInvokedThenSelectedAlertWasSetToNull()
        {
            var alertsControlViewModel = new AlertsControlViewModel(this.smartDetectorRunner);
            var emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert());

            alertsControlViewModel.SelectedAlert = emulationAlert;

            Assert.IsNotNull(alertsControlViewModel.AlertDetailsControlViewModel, "Alert details control view model should not be null");
            Assert.AreEqual(emulationAlert, alertsControlViewModel.AlertDetailsControlViewModel.Alert, "Unexpected alert details control view model");

            alertsControlViewModel.AlertDetailsControlViewModel.CloseControlCommand.Execute(parameter: null);

            Assert.IsNull(alertsControlViewModel.SelectedAlert, "Selected alert should be null");
        }

        private class TestAlert : Alert
        {
            public TestAlert()
                : base("Test title", new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM"), DateTime.UtcNow)
            {
            }
        }
    }
}
