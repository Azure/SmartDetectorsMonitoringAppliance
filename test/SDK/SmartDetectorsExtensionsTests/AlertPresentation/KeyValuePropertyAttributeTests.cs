//-----------------------------------------------------------------------
// <copyright file="KeyValuePropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    [TestClass]
    public class KeyValuePropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenKeyValuePropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(3, contractsAlert.AlertProperties.Count);

            int propertyIndex = 1;
            Assert.AreEqual("KeyValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("KeyValueDisplayName", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("value1", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value["key1"]);
            Assert.AreEqual(false, ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ShowHeaders);

            propertyIndex++;
            Assert.AreEqual("KeyValueWithHeaders", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("KeyValueWithHeadersDisplayName", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("value1", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value["key1"]);
            Assert.AreEqual(true, ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ShowHeaders);
            Assert.AreEqual("Keys", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).KeyHeaderName);
            Assert.AreEqual("Values1", ((KeyValueAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ValueHeaderName);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertWithReferencePropertiesThenKeyValuePropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferenceProperties>();

            Assert.AreEqual(3, contractsAlert.AlertProperties.Count);

            int propertyIndex = 1;
            Assert.AreEqual("KeyValueReference", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("KeyValueReferenceDisplayName", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("keyValueReferencePath", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ReferencePath);
            Assert.AreEqual(false, ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ShowHeaders);

            propertyIndex++;
            Assert.AreEqual("KeyValueWithHeadersReference", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("KeyValueWithHeadersReferenceDisplayName", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("keyValueWithHeadersReferencePath", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ReferencePath);
            Assert.AreEqual(true, ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ShowHeaders);
            Assert.AreEqual("Keys", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).KeyHeaderName);
            Assert.AreEqual("Values2", ((KeyValueReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ValueHeaderName);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithInvalidKeyValuePropertyThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithInvalidKeyValueProperty>();
        }

        public class TestAlert : TestAlertBase
        {
            public int RawProperty => 1;

            [KeyValueProperty("KeyValueDisplayName")]
            public IDictionary<string, string> KeyValue => new Dictionary<string, string> { { "key1", "value1" } };

            [KeyValueProperty("KeyValueWithHeadersDisplayName", "Keys", "Values{RawProperty}")]
            public IDictionary<string, string> KeyValueWithHeaders => new Dictionary<string, string> { { "key1", "value1" } };
        }

        public class TestAlertWithReferenceProperties : TestAlertBase
        {
            public int RawProperty => 2;

            [KeyValueProperty("KeyValueReferenceDisplayName")]
            public PropertyReference KeyValueReference => new PropertyReference("keyValueReferencePath");

            [KeyValueProperty("KeyValueWithHeadersReferenceDisplayName", "Keys", "Values{RawProperty}")]
            public PropertyReference KeyValueWithHeadersReference => new PropertyReference("keyValueWithHeadersReferencePath");
        }

        public class TestAlertWithInvalidKeyValueProperty : TestAlertBase
        {
            [KeyValueProperty("KeyValueDisplayName")]
            public string KeyValue => "Oops";
        }
    }
}
