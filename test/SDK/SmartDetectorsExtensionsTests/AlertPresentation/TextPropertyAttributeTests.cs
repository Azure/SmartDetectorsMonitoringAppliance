//-----------------------------------------------------------------------
// <copyright file="TextPropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    [TestClass]
    public class TextPropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenTextPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertBasic>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            Assert.AreEqual("LongTextValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
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

        [TestMethod]
        public void WhenCreatingContractsAlertWithFormatStringThenTextPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithFormatString>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            Assert.AreEqual("DateTimeValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("DateTimeDisplayName", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("The time is: 2019-03-07 11", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("DoubleValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("DoubleDisplayName", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("The area of the unit circle is 3, or 3.141593 to be exact", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertWithReferencePropertiesThenTextPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferenceProperties>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            Assert.AreEqual("LongTextReference", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("LongTextReferenceDisplayName", ((LongTextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((LongTextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("longTextReferencePath", ((LongTextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ReferencePath);

            propertyIndex++;
            Assert.AreEqual("TextReference", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("TextReferenceDisplayName", ((TextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((TextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("textReferencePath", ((TextReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex]).ReferencePath);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithReferenceTextPropertyAndFormatStringThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferenceTextAndFormatString>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithReferenceLongTextPropertyAndFormatStringThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferenceLongTextAndFormatString>();
        }

        public class TestAlertBasic : TestAlertBase
        {
            [LongTextProperty("LongTextDisplayName")]
            public string LongTextValue => "LongTextValue";

            [TextProperty("TextDisplayName")]
            public string TextValue => "TextValue";
        }

        public class TestAlertWithFormatString : TestAlertBase
        {
            [TextProperty("DateTimeDisplayName", FormatString = "The time is: {0:yyyy-MM-dd HH}")]
            public DateTime DateTimeValue => new DateTime(2019, 3, 7, 11, 23, 47);

            [LongTextProperty("DoubleDisplayName", FormatString = "The area of the unit circle is {0:F0}, or {0:F6} to be exact")]
            public double DoubleValue => Math.PI;
        }

        public class TestAlertWithReferenceProperties : TestAlertBase
        {
            [TextProperty("TextReferenceDisplayName")]
            public PropertyReference TextReference => new PropertyReference("textReferencePath");

            [LongTextProperty("LongTextReferenceDisplayName")]
            public PropertyReference LongTextReference => new PropertyReference("longTextReferencePath");
        }

        public class TestAlertWithReferenceTextAndFormatString : TestAlertBase
        {
            [TextProperty("TextReferenceDisplayName", FormatString = "SomeString")]
            public PropertyReference TextReference => new PropertyReference("textReferencePath");
        }

        public class TestAlertWithReferenceLongTextAndFormatString : TestAlertBase
        {
            [LongTextProperty("LongTextReferenceDisplayName", FormatString = "SomeString")]
            public PropertyReference LongTextReference => new PropertyReference("longTextReferencePath");
        }
    }
}
