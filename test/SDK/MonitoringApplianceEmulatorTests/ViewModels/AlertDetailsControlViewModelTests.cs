//-----------------------------------------------------------------------
// <copyright file="AlertDetailsControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using AlertState = Microsoft.Azure.Monitoring.SmartDetectors.AlertState;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertDetailsControlViewModelTests
    {
        private readonly ResourceIdentifier virtualMachineResourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
        private readonly ResourceIdentifier appInsightsResourceIdentifier = new ResourceIdentifier(ResourceType.ApplicationInsights, "someSubscription", "someGroup", "someApp");

        private Mock<ISystemProcessClient> systemProcessClientMock;

        [TestInitialize]
        public void Setup()
        {
            this.systemProcessClientMock = new Mock<ISystemProcessClient>();
        }

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert(this.virtualMachineResourceIdentifier));
            bool wasCloseEventHandlerFired = false;

            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () =>
                {
                    wasCloseEventHandlerFired = true;
                },
                this.systemProcessClientMock.Object);

            // Verify "Essentials" properties
            Assert.AreEqual("Subscription id", alertDetailsControlViewModel.EssentialsSectionProperties[0].ResourceType, "Unexpected essential property 'Subscription id'");
            Assert.AreEqual("someSubscription", alertDetailsControlViewModel.EssentialsSectionProperties[0].ResourceName, "Unexpected essential property 'Subscription id'");

            Assert.AreEqual("Resource group", alertDetailsControlViewModel.EssentialsSectionProperties[1].ResourceType, "Unexpected essential property 'Resource group'");
            Assert.AreEqual("someGroup", alertDetailsControlViewModel.EssentialsSectionProperties[1].ResourceName, "Unexpected essential property 'Resource group'");

            Assert.AreEqual("Resource type", alertDetailsControlViewModel.EssentialsSectionProperties[2].ResourceType, "Unexpected essential property 'Resource type'");
            Assert.AreEqual("VirtualMachine", alertDetailsControlViewModel.EssentialsSectionProperties[2].ResourceName, "Unexpected essential property 'Resource type'");

            Assert.AreEqual("Resource name", alertDetailsControlViewModel.EssentialsSectionProperties[3].ResourceType, "Unexpected essential property 'Resource name'");
            Assert.AreEqual("someVM", alertDetailsControlViewModel.EssentialsSectionProperties[3].ResourceName, "Unexpected essential property 'Resource name'");

            // Verify "Details" properties
            Assert.AreEqual(4, alertDetailsControlViewModel.DisplayableProperties.Count, "Unexpected count of displayable properties");
            for (var index = 0; index < alertDetailsControlViewModel.DisplayableProperties.Count - 1; index++)
            {
                string invalidOrderMessage =
                    $"Unexpected order of details section properties: Order of property in {index} index is {alertDetailsControlViewModel.DisplayableProperties[index].Order}, " +
                    $"while order of property in {index + 1} index is {alertDetailsControlViewModel.DisplayableProperties[index + 1].Order}";

                Assert.IsTrue(
                    alertDetailsControlViewModel.DisplayableProperties[index].Order <= alertDetailsControlViewModel.DisplayableProperties[index + 1].Order,
                    invalidOrderMessage);
            }

            VerifyAlertProperty(
                new TextAlertProperty("TextProperty2", "Some numeric string", 1, "5"),
                alertDetailsControlViewModel.DisplayableProperties[0]);

            VerifyAlertProperty(
                new TextAlertProperty("TextProperty1", "Some string", 2, "Ahlan world"),
                alertDetailsControlViewModel.DisplayableProperties[1]);

            VerifyAlertProperty(
                new KeyValueAlertProperty("KeyValue1", "PresidentsKeyValue", 3, "First name", "Last name", new Dictionary<string, string>() { { "Donald", "Trump" }, { "Barak", "Obama" } }),
                alertDetailsControlViewModel.DisplayableProperties[2]);

            VerifyAlertProperty(
                new KeyValueAlertProperty("KeyValue2", "PlayersKeyValue", 4, new Dictionary<string, string>() { { "Yaniv", "Katan" }, { "Avishai", "Zano" } }),
                alertDetailsControlViewModel.DisplayableProperties[3]);

            // Verify close event was fired
            Assert.IsFalse(wasCloseEventHandlerFired);
            alertDetailsControlViewModel.CloseControlCommand.Execute(parameter: null);
            Assert.IsTrue(wasCloseEventHandlerFired);
        }

        [TestMethod]
        public void WhenExecutingOpenAnalyticsQueryCommandForNonAppInsightsResourceThenQueryWasExecutedAsExpected()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert(this.virtualMachineResourceIdentifier));

            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () => { },
                this.systemProcessClientMock.Object);

            alertDetailsControlViewModel.OpenAnalyticsQueryCommand.Execute(parameter: "<query>");

            string expectedAbsoluteUri = "https://portal.loganalytics.io/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourcegroups/MyResourceGroupName/workspaces/MyVirtualMachineName?q=H4sIAAAAAAAEALMpLE0tqrQDAJjF8mcHAAAA";

            // Verify that the query was composed and executed as expected
            this.systemProcessClientMock.Verify(m => m.StartWebBrowserProcess(It.Is<Uri>(u => u.AbsoluteUri == expectedAbsoluteUri)), Times.Once());
        }

        [TestMethod]
        public void WhenExecutingOpenAnalyticsQueryCommandForAppInsightsResourceThenQueryWasExecutedAsExpected()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert(this.appInsightsResourceIdentifier));

            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () => { },
                this.systemProcessClientMock.Object);

            alertDetailsControlViewModel.OpenAnalyticsQueryCommand.Execute(parameter: "<query>");

            string expectedAbsoluteUri = "https://analytics.applicationinsights.io/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourcegroups/MyResourceGroupName/components/someApp?q=H4sIAAAAAAAEALMpLE0tqrQDAJjF8mcHAAAA";

            // Verify that the query was composed and executed as expected
            this.systemProcessClientMock.Verify(m => m.StartWebBrowserProcess(It.Is<Uri>(u => u.AbsoluteUri == expectedAbsoluteUri)), Times.Once());
        }

        private static void VerifyAlertProperty(DisplayableAlertProperty expected, DisplayableAlertProperty actual)
        {
            Assert.AreEqual(expected.PropertyName, actual.PropertyName, $"Unexpected value for '{nameof(expected.PropertyName)}' property");
            Assert.AreEqual(expected.DisplayName, actual.DisplayName, $"Unexpected value for '{nameof(expected.DisplayName)}' property");
            Assert.AreEqual(expected.Order, actual.Order, $"Unexpected value for '{nameof(expected.Order)}' property");
            Assert.AreEqual(expected.Type, actual.Type, $"Unexpected value for '{nameof(expected.Type)}' property");

            switch (expected.Type)
            {
                case AlertPropertyType.Text:
                    VerifyAlertTextProperty(expected as TextAlertProperty, actual as TextAlertProperty);
                    break;
                case AlertPropertyType.KeyValue:
                    VerifyAlertKeyValueProperty(expected as KeyValueAlertProperty, actual as KeyValueAlertProperty);
                    break;
            }
        }

        private static void VerifyAlertTextProperty(TextAlertProperty expected, TextAlertProperty actual)
        {
            Assert.AreEqual(expected.Value, actual.Value, $"Unexpected value for '{nameof(expected.Value)}' property");
        }

        private static void VerifyAlertKeyValueProperty(KeyValueAlertProperty expected, KeyValueAlertProperty actual)
        {
            CollectionAssert.AreEquivalent(expected.Value.ToDictionary(t => t.Key, t => t.Value), actual.Value.ToDictionary(t => t.Key, t => t.Value), $"Unexpected value for '{nameof(expected.Value)}' property");
            Assert.AreEqual(expected.KeyHeaderName, actual.KeyHeaderName, $"Unexpected value for '{nameof(expected.KeyHeaderName)}' property");
            Assert.AreEqual(expected.ValueHeaderName, actual.ValueHeaderName, $"Unexpected value for '{nameof(expected.ValueHeaderName)}' property");
            Assert.AreEqual(expected.ShowHeaders, actual.ShowHeaders, $"Unexpected value for '{nameof(expected.ShowHeaders)}' property");
        }

        public class TestAlert : Alert
        {
            public TestAlert(ResourceIdentifier resourceIdentifier)
                : base("Test title", resourceIdentifier, AlertState.Active)
            {
                this.TextProperty1 = "Ahlan world";
                this.TextProperty2 = 5;
                this.KeyValue1 = new Dictionary<string, string>() { { "Donald", "Trump" }, { "Barak", "Obama" } };
                this.KeyValue2 = new Dictionary<string, string>() { { "Yaniv", "Katan" }, { "Avishai", "Zano" } };
                this.NoPresentation = "no show";
            }

            [AlertPresentationKeyValue("PresidentsKeyValue", "First name", "Last name", Order = 3)]
            public IDictionary<string, string> KeyValue1 { get; }

            [AlertPresentationKeyValue("PlayersKeyValue", Order = 4)]
            public IDictionary<string, string> KeyValue2 { get; }

            [AlertPresentationText("Some string", Order = 2)]
            public string TextProperty1 { get; }

            [AlertPresentationText("Some numeric string", Order = 1)]
            public int TextProperty2 { get; }

            public string NoPresentation { get; set; }
        }
    }
}
