//-----------------------------------------------------------------------
// <copyright file="NotificationServiceTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NotificationServiceTests
    {
        [TestMethod]
        public void WhenMethodIsRegisteredTabSwitchedToAlertsControlEventThenMethodWasInvokedWhenExpected()
        {
            MainWindowTabItem selectedTab = MainWindowTabItem.SmartDetectorConfigurationControl;

            var notificationService = new NotificationService();

            notificationService.TabSwitchedToAlertsControl += () => { selectedTab = MainWindowTabItem.AlertsControl; };

            notificationService.OnTabSwitchedToAlertsControl();

            Assert.AreEqual(MainWindowTabItem.AlertsControl, selectedTab, "Unexpected value of selected tab");
        }
    }
}
