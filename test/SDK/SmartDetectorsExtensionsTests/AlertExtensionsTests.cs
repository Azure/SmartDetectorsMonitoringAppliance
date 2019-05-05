//-----------------------------------------------------------------------
// <copyright file="AlertExtensionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Test code, approved")]
namespace SmartDetectorsExtensionsTests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// Test class for testing general alert conversions implemented in the <see cref="AlertExtensions"/> class.
    /// Individual presentation attributes conversion is tested in dedicated test classes.
    /// </summary>
    [TestClass]
    public class AlertExtensionsTests
    {
        private const string SmartDetectorName = "smartDetectorName";
        private SmartDetectorAnalysisRequest analysisRequest;

        [TestInitialize]
        public void TestInitialize()
        {
            this.analysisRequest = new SmartDetectorAnalysisRequest
            {
                ResourceIds = new List<string>() { "resourceId" },
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromDays(1),
            };
        }

        [TestMethod]
        public void WhenCreatingContractsAlertThenAlertDataIsCorrect()
        {
            var resourceId = new ResourceIdentifier(ResourceType.ApplicationInsights, "subscription", "resourceGroup", "myApp");
            var alert = new TestAlert(resourceIdentifier: resourceId);

            ContractsAlert contractsAlert = alert.CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);

            Assert.AreEqual("AlertTitle", contractsAlert.Title);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Insights/components/myApp", contractsAlert.ResourceId);
            Assert.AreEqual(alert.OccurenceTime, contractsAlert.OccurenceTime);
            Assert.AreEqual("smartDetectorId", contractsAlert.SmartDetectorId);
            Assert.AreEqual(SmartDetectorName, contractsAlert.SmartDetectorName);
            Assert.AreEqual((int)this.analysisRequest.Cadence.TotalMinutes, contractsAlert.AnalysisWindowSizeInMinutes);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType);
            Assert.IsNull(contractsAlert.ResolutionParameters);
            this.AssertAlertProperties(contractsAlert);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertWithResolutionPArametersThenAlertDataIsCorrect()
        {
            var resourceId = new ResourceIdentifier(ResourceType.ApplicationInsights, "subscription", "resourceGroup", "myApp");
            var alert = new TestAlert(resourceIdentifier: resourceId)
            {
                AlertResolutionParameters = new AlertResolutionParameters
                {
                    CheckForResolutionAfter = TimeSpan.FromMinutes(9)
                }
            };

            ContractsAlert contractsAlert = alert.CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);

            Assert.AreEqual("AlertTitle", contractsAlert.Title);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Insights/components/myApp", contractsAlert.ResourceId);
            Assert.AreEqual(alert.OccurenceTime, contractsAlert.OccurenceTime);
            Assert.AreEqual("smartDetectorId", contractsAlert.SmartDetectorId);
            Assert.AreEqual(SmartDetectorName, contractsAlert.SmartDetectorName);
            Assert.AreEqual((int)this.analysisRequest.Cadence.TotalMinutes, contractsAlert.AnalysisWindowSizeInMinutes);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType);
            Assert.AreEqual(9.0, contractsAlert.ResolutionParameters.CheckForResolutionAfter.TotalMinutes);
            this.AssertAlertProperties(contractsAlert);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertAndAlertsHaveIdenticalPredicatesThenTheCorrelationHashIsSame()
        {
            // A non predicate property is different - correlation hash should be the same
            ContractsAlert contractsAlert1 = new TestAlert().CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);
            ContractsAlert contractsAlert2 = new TestAlert(rawProperty: 2).CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);
            Assert.AreEqual(contractsAlert1.CorrelationHash, contractsAlert2.CorrelationHash);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertAndAlertsHaveDifferentPredicatesThenTheCorrelationHashIsDifferent()
        {
            // A predicate property is different - correlation hash should be the different
            ContractsAlert contractsAlert = new TestAlert().CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);
            ContractsAlert contractsAlertDifferentPredicate = new TestAlert(title: "AlertTitle2").CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, false);
            Assert.AreNotEqual(contractsAlert.CorrelationHash, contractsAlertDifferentPredicate.CorrelationHash);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertAndMetricClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = new TestAlert().CreateContractsAlert(this.analysisRequest, SmartDetectorName, false, true);
            Assert.AreEqual(SignalType.Metric, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenCreatingContractsAlertAndLogClientWasUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = new TestAlert().CreateContractsAlert(this.analysisRequest, SmartDetectorName, true, false);
            Assert.AreEqual(SignalType.Log, contractsAlert.SignalType, "Unexpected signal type");
        }

        [TestMethod]
        public void WhenCreatingContractsAlertAndLogAndMetricClientsWereUsedThenTheSignalTypeIsCorrect()
        {
            ContractsAlert contractsAlert = new TestAlert().CreateContractsAlert(this.analysisRequest, SmartDetectorName, true, true);
            Assert.AreEqual(SignalType.Multiple, contractsAlert.SignalType, "Unexpected signal type");
        }

        private void AssertAlertProperties(ContractsAlert contractsAlert)
        {
            Assert.AreEqual(4, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            Assert.AreEqual("Predicate", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Raw, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("AlertTitle", ((RawAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("RawProperty", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Raw, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual(1, ((RawAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("LongTextPropertyName", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("LongTextDisplayName", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("LongTextValue", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("TextValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("TextDisplayName", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("TextValue", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);
        }

        private class TestAlert : Alert
        {
            public TestAlert(int rawProperty = 1, string title = "AlertTitle", ResourceIdentifier resourceIdentifier = default(ResourceIdentifier))
                : base(title, resourceIdentifier)
            {
                this.RawProperty = rawProperty;
            }

            [PredicateProperty]
            public string Predicate => this.Title;

            public int RawProperty { get; }

            [TextProperty("TextDisplayName", Order = 2)]
            public string TextValue => "TextValue";

            [LongTextProperty("LongTextDisplayName", Order = 0, PropertyName = "LongTextPropertyName")]
            public string LongTextValue => "LongTextValue";
        }
    }
}
