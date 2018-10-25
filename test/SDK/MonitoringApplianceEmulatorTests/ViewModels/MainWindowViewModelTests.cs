//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MainWindowViewModelTests
    {
        private Mock<IAuthenticationServices> authenticationServicesMock;

        private Mock<IEmulationSmartDetectorRunner> smartDetectorRunnerMock;

        private NotificationService notificationService;

        [TestInitialize]
        public void Setup()
        {
            this.authenticationServicesMock = new Mock<IAuthenticationServices>();

            this.smartDetectorRunnerMock = new Mock<IEmulationSmartDetectorRunner>();

            this.notificationService = new NotificationService();
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            var mainWindowModel = new MainWindowViewModel(this.authenticationServicesMock.Object, this.smartDetectorRunnerMock.Object, this.notificationService);

            Assert.AreEqual(this.authenticationServicesMock.Object, mainWindowModel.AuthenticationServices, "Unexpected authentication services");
            Assert.AreEqual(this.smartDetectorRunnerMock.Object, mainWindowModel.SmartDetectorRunner, "Unexpected smart detector runner");
            Assert.AreEqual(MainWindowTabItem.SmartDetectorConfigurationControl, mainWindowModel.SelectedTab, "Unexpected selected tab");
        }

        [TestMethod]
        public void WhenTabSwitchToAlertsControlEventWasInvokedThenTheSelectedTabWasChangedToAlertsControl()
        {
            var mainWindowModel = new MainWindowViewModel(this.authenticationServicesMock.Object, this.smartDetectorRunnerMock.Object, this.notificationService);

            Assert.AreEqual(MainWindowTabItem.SmartDetectorConfigurationControl, mainWindowModel.SelectedTab, "Unexpected selected tab");

            this.notificationService.OnTabSwitchedToAlertsControl();

            Assert.AreEqual(MainWindowTabItem.AlertsControl, mainWindowModel.SelectedTab, "Unexpected selected tab");
        }
    }
}
