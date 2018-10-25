//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EmulationStatusControlViewModelTests
    {
        private Mock<IEmulationSmartDetectorRunner> smartDetectorRunnerMock;

        private Mock<IObservableTracer> tracerMock;

        private NotificationService notificationService;

        [TestInitialize]
        public void Setup()
        {
           this.smartDetectorRunnerMock = new Mock<IEmulationSmartDetectorRunner>();

           this.tracerMock = new Mock<IObservableTracer>();

           this.notificationService = new NotificationService();
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            var emulationStatusControlViewModel = new EmulationStatusControlViewModel(this.smartDetectorRunnerMock.Object, this.tracerMock.Object, this.notificationService);

            Assert.AreEqual(this.smartDetectorRunnerMock.Object, emulationStatusControlViewModel.SmartDetectorRunner, "Unexpected smart detector runner");
            Assert.AreEqual(this.tracerMock.Object, emulationStatusControlViewModel.Tracer, "Unexpected tracer");
        }

        [TestMethod]
        public void WhenExecutingClearTracerBoxCommandCommandThenTheTracerBoxWasCleared()
        {
            var emulationStatusControlViewModel = new EmulationStatusControlViewModel(this.smartDetectorRunnerMock.Object, this.tracerMock.Object, this.notificationService);

            emulationStatusControlViewModel.ClearTracerBoxCommand.Execute(parameter: null);

            this.tracerMock.Verify(m => m.Clear(), Times.Once());
        }

        [TestMethod]
        public void WhenExecutingSwitchTabCommandThenTheTabWasSwitched()
        {
            var emulationStatusControlViewModel = new EmulationStatusControlViewModel(this.smartDetectorRunnerMock.Object, this.tracerMock.Object, this.notificationService);

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
