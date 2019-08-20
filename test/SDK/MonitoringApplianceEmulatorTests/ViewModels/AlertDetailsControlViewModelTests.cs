//-----------------------------------------------------------------------
// <copyright file="AlertDetailsControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
[assembly: System.Resources.NeutralResourcesLanguage("en")]

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class AlertDetailsControlViewModelTests
    {
        private readonly ResourceIdentifier virtualMachineResourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");

        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlert(this.virtualMachineResourceIdentifier));
            bool wasCloseEventHandlerFired = false;

            var armClientMock = new Mock<IAzureResourceManagerClient>();
            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () =>
                {
                    wasCloseEventHandlerFired = true;
                },
                armClientMock.Object);

            var alertPropreties = alertDetailsControlViewModel.DisplayablePropertiesTask.Result;

            // Verify "Essentials" properties
            AssertEssentialProperties(alertDetailsControlViewModel.EssentialsSectionProperties);

            // Verify "Details" properties
            Assert.AreEqual(5, alertPropreties.Count, "Unexpected count of displayable properties");
            for (var index = 0; index < alertPropreties.Count - 1; index++)
            {
                string invalidOrderMessage =
                    $"Unexpected order of details section properties: Order of property in {index} index is {alertPropreties[index].Order}, " +
                    $"while order of property in {index + 1} index is {alertPropreties[index + 1].Order}";

                Assert.IsTrue(
                    alertPropreties[index].Order <= alertPropreties[index + 1].Order,
                    invalidOrderMessage);
            }

            // Verify close event was fired
            Assert.IsFalse(wasCloseEventHandlerFired);
            alertDetailsControlViewModel.CloseControlCommand.Execute(parameter: null);
            Assert.IsTrue(wasCloseEventHandlerFired);
        }

       [TestMethod]
        public void WhenCreatingNewViewModelWithArmRequestThenItWasInitializedCorrectly()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlertWithArm(this.virtualMachineResourceIdentifier));
            bool wasCloseEventHandlerFired = false;

            string responseContent = (Encoding.Default.GetString(ArmResponses.ActivityLogResponse));
            JObject responseObject = JObject.Parse(responseContent);
            List<JObject> mockResponse = new List<JObject>(responseObject["value"].ToObject<List<JObject>>());
            string subscriptionId = emulationAlert.ResourceIdentifier.SubscriptionId;

            var armClientMock = new Mock<IAzureResourceManagerClient>();
            armClientMock
                .Setup(m => m.ExecuteArmQueryAsync(new Uri("subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01", UriKind.Relative), CancellationToken.None))
                .ReturnsAsync(mockResponse);

            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () =>
                {
                    wasCloseEventHandlerFired = true;
                },
                armClientMock.Object);

            var alertPropreties = alertDetailsControlViewModel.DisplayablePropertiesTask.Result;

            // Verify "Essentials" properties
            AssertEssentialProperties(alertDetailsControlViewModel.EssentialsSectionProperties);

            // Verify "Details" properties
            AssertAlertProperties(alertPropreties);
            Assert.AreEqual(7, alertPropreties.Count, "Unexpected count of displayable properties");
            for (var index = 0; index < alertPropreties.Count - 1; index++)
            {
                string invalidOrderMessage =
                    $"Unexpected order of details section properties: Order of property in {index} index is {alertPropreties[index].Order}, " +
                    $"while order of property in {index + 1} index is {alertPropreties[index + 1].Order}";

                Assert.IsTrue(
                    alertPropreties[index].Order <= alertPropreties[index + 1].Order,
                    invalidOrderMessage);
            }

            // Verify close event was fired
            Assert.IsFalse(wasCloseEventHandlerFired);
            alertDetailsControlViewModel.CloseControlCommand.Execute(parameter: null);
            Assert.IsTrue(wasCloseEventHandlerFired);
        }

        [TestMethod]
        public void WhenCreatingNewViewModelWithFailedArmRequestThenItWasInitializedCorrectly()
        {
            EmulationAlert emulationAlert = EmulationAlertHelper.CreateEmulationAlert(new TestAlertWithArm(this.virtualMachineResourceIdentifier));
            bool wasCloseEventHandlerFired = false;

            string responseContent = (Encoding.Default.GetString(ArmResponses.ActivityLogResponse));
            JObject responseObject = JObject.Parse(responseContent);
            List<JObject> mockResponse = new List<JObject>(responseObject["value"].ToObject<List<JObject>>());
            string subscriptionId = emulationAlert.ResourceIdentifier.SubscriptionId;

            var armClientMock = new Mock<IAzureResourceManagerClient>();
            armClientMock
                .Setup(m => m.ExecuteArmQueryAsync(new Uri("subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01", UriKind.Relative), CancellationToken.None))
                .Throws(new HttpRequestException("Query returned an error code 500"));

            var alertDetailsControlViewModel = new AlertDetailsControlViewModel(
                emulationAlert,
                () =>
                {
                    wasCloseEventHandlerFired = true;
                },
                armClientMock.Object);

            var alertPropreties = alertDetailsControlViewModel.DisplayablePropertiesTask.Result;

            // Verify "Essentials" properties
            AssertEssentialProperties(alertDetailsControlViewModel.EssentialsSectionProperties);

            // Verify "Details" properties
            Assert.AreEqual(7, alertPropreties.Count, "Unexpected count of displayable properties");
            int index = 0;

            // Verify the none Arm properties have the correct information
            Assert.AreEqual("BeforeArmLongTextReference", alertPropreties[index].DisplayName, "unexpected Display name in none ARM property");
            Assert.AreEqual("longTextReferencePathNotArm", ((LongTextAlertProperty)alertPropreties[index]).Value, "unexpected value set in none ARM property");

            // Verify the Arm properties are returned with error response in value
            index++;
            Assert.AreEqual("TextReferenceDisplayName", alertPropreties[index].DisplayName, "unexpected Display name in ARM property");
            Assert.AreEqual("Failed to get Arm Response, Error: Query returned an error code 500", ((TextAlertProperty)alertPropreties[index]).Value, "unexpected value set in ARM property");

            // Verify close event was fired
            Assert.IsFalse(wasCloseEventHandlerFired);
            alertDetailsControlViewModel.CloseControlCommand.Execute(parameter: null);
            Assert.IsTrue(wasCloseEventHandlerFired);
        }

        private static void AssertAlertProperties(System.Collections.ObjectModel.ObservableCollection<DisplayableAlertProperty> displayableAlerts)
        {
            Assert.AreEqual(7, displayableAlerts.Count);

            int propertyIndex = 0;

            Assert.AreEqual("BeforeArmLongTextReference", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual(AlertPropertyType.LongText, displayableAlerts[propertyIndex].Type);
            Assert.AreEqual("longTextReferencePathNotArm", ((LongTextAlertProperty)displayableAlerts[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("TextReferenceDisplayName", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual("TextReference", displayableAlerts[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, displayableAlerts[propertyIndex].Type);
            Assert.AreEqual("VSDROP", ((TextAlertProperty)displayableAlerts[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("LongTextReferenceDisplayName", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual("LongTextReference", displayableAlerts[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, displayableAlerts[propertyIndex].Type);
            Assert.AreEqual("/subscriptions/d03b04c7-d1d4-467b-aaaa-87b6fcb38b38/resourceGroups/VSDROP/providers/Microsoft.Web/serverFarms/VSDropAppServicePlan", ((LongTextAlertProperty)displayableAlerts[propertyIndex]).Value);

            // When order is fixed this should be at the end of the response
            propertyIndex++;
            Assert.AreEqual("AfterArmTextReference", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual("TextReference", displayableAlerts[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, displayableAlerts[propertyIndex].Type);
            Assert.AreEqual("textReferencePathNotArm", ((TextAlertProperty)displayableAlerts[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("KeyValueReferenceDisplayName", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual("KeyValueReference", displayableAlerts[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, displayableAlerts[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("MultiColumnTableReferenceDisplayName", displayableAlerts[propertyIndex].DisplayName);
            Assert.AreEqual("MultiColumnTableReference", displayableAlerts[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, displayableAlerts[propertyIndex].Type);
            Assert.AreEqual(20, ((TableAlertProperty<Dictionary<string, JToken>>)displayableAlerts[propertyIndex]).Values.Count);
            Assert.AreEqual(3, ((TableAlertProperty<Dictionary<string, JToken>>)displayableAlerts[propertyIndex]).Columns.Count);
        }

        private static void AssertEssentialProperties(System.Collections.ObjectModel.ObservableCollection<AzureResourceProperty> essentialsProperties)
        {
            Assert.AreEqual("Subscription id", essentialsProperties[0].ResourceType, "Unexpected essential property 'Subscription id'");
            Assert.AreEqual("someSubscription", essentialsProperties[0].ResourceName, "Unexpected essential property 'Subscription id'");

            Assert.AreEqual("Resource group", essentialsProperties[1].ResourceType, "Unexpected essential property 'Resource group'");
            Assert.AreEqual("someGroup", essentialsProperties[1].ResourceName, "Unexpected essential property 'Resource group'");

            Assert.AreEqual("Resource type", essentialsProperties[2].ResourceType, "Unexpected essential property 'Resource type'");
            Assert.AreEqual("VirtualMachine", essentialsProperties[2].ResourceName, "Unexpected essential property 'Resource type'");

            Assert.AreEqual("Resource name", essentialsProperties[3].ResourceType, "Unexpected essential property 'Resource name'");
            Assert.AreEqual("someVM", essentialsProperties[3].ResourceName, "Unexpected essential property 'Resource name'");
        }

        public class TestAlert : Alert
        {
            public TestAlert(ResourceIdentifier resourceIdentifier)
                : base("Test title", resourceIdentifier)
            {
                this.TextProperty1 = "Ahlan world";
                this.TextProperty2 = 5;
                this.KeyValue1 = new Dictionary<string, string>() { { "Donald", "Trump" }, { "Barak", "Obama" } };
                this.KeyValue2 = new Dictionary<string, string>() { { "Yaniv", "Katan" }, { "Avishai", "Zano" } };
                this.NoPresentation = "no show";
                this.TableProp = new List<TestTableAlertPropertyValue>()
                {
                    new TestTableAlertPropertyValue() { FirstName = "Edinson", LastName = "Cavani", Goals = 4.67 },
                    new TestTableAlertPropertyValue() { FirstName = "Fernando", LastName = "Torres", Goals = 1.7 }
                };
            }

            [KeyValueProperty("PresidentsKeyValue", "First name", "Last name", Order = 3)]
            public IDictionary<string, string> KeyValue1 { get; }

            [KeyValueProperty("PlayersKeyValue", Order = 4)]
            public IDictionary<string, string> KeyValue2 { get; }

            [TextProperty("Some string", Order = 2)]
            public string TextProperty1 { get; }

            [TextProperty("Some numeric string", Order = 1)]
            public int TextProperty2 { get; }

            public string NoPresentation { get; }

            [MultiColumnTableProperty("Some Table", Order = 5)]
            public List<TestTableAlertPropertyValue> TableProp { get; }
        }

        public class TestAlertWithArm : Alert
        {
            public TestAlertWithArm(ResourceIdentifier resourceIdentifier)
                : base("Arm Test title", resourceIdentifier)
            {
                this.ActivityLog = new TestArmRequestActivityLog();
            }

            [TextProperty("Display name")]
            public string MyText { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [LongTextProperty("BeforeArmLongTextReference", Order = 2)]
            public string LongTextReference => "longTextReferencePathNotArm";

            [AzureResourceManagerRequestProperty]
            public TestArmRequestActivityLog ActivityLog { get; }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [TextProperty("AfterArmTextReference", Order = 9)]
            public string TextReference => "textReferencePathNotArm";
        }

        public class TestArmRequestActivityLog : AzureResourceManagerRequest
        {
            public TestArmRequestActivityLog()
                : base(new Uri("subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01", UriKind.Relative))
            {
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [TextProperty("TextReferenceDisplayName", Order = 1)]
            public PropertyReference TextReference => new PropertyReference("resourceGroupName");

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [TextProperty("TextReferenceTheSecond", Order = 5)]
            public PropertyReference TextReference2 => new PropertyReference("eventName.value");

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [LongTextProperty("LongTextReferenceDisplayName", Order = 2)]
            public PropertyReference LongTextReference => new PropertyReference("authorization.scope");

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [KeyValueProperty("KeyValueReferenceDisplayName", Order = 3)]
            public PropertyReference KeyValueReference => new PropertyReference("eventDataId");

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", Order = 4, ShowHeaders = true)]
            public TablePropertyReference<ReferenceTableDataActivityLog> MultiColumnTableReference => new TablePropertyReference<ReferenceTableDataActivityLog>("$");
        }

        public class ReferenceTableDataActivityLog
        {
            [JsonProperty("operationId")]
            [TableColumn("Operation Id", "operationId", Order = 1)]
            public string OperationId { get; set; }

            [JsonProperty("level")]
            [TableColumn("Level", "level", Order = 1)]
            public string Level { get; set; }

            [JsonProperty("eventTimestamp")]
            [TableColumn("operation time", "eventTimestamp", Order = 1)]
            public string EventTimestamp { get; set; }
        }
    }
}
