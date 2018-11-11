//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EmulationStatusControlViewModelTests
    {
        private Mock<IEmulationSmartDetectorRunner> smartDetectorRunnerMock;

        private NotificationService notificationService;

        [TestInitialize]
        public void Setup()
        {
           this.smartDetectorRunnerMock = new Mock<IEmulationSmartDetectorRunner>();

           this.notificationService = new NotificationService();
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            var emulationStatusControlViewModel = new EmulationStatusControlViewModel(this.smartDetectorRunnerMock.Object, this.notificationService);

            Assert.AreEqual(this.smartDetectorRunnerMock.Object, emulationStatusControlViewModel.SmartDetectorRunner, "Unexpected smart detector runner");
        }

        [TestMethod]
        public void WhenExecutingSwitchTabCommandThenTheTabWasSwitched()
        {
            var emulationStatusControlViewModel = new EmulationStatusControlViewModel(this.smartDetectorRunnerMock.Object, this.notificationService);

            bool tabSwitchedToAlertsControlEventWasFired = false;

            this.notificationService.TabSwitchedToAlertsControl += () =>
                {
                    tabSwitchedToAlertsControlEventWasFired = true;
                };

            emulationStatusControlViewModel.SwitchTabCommand.Execute(parameter: null);

            Assert.IsTrue(tabSwitchedToAlertsControlEventWasFired);
        }
    }
}
