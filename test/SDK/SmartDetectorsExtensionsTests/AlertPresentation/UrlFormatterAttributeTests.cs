//-----------------------------------------------------------------------
// <copyright file="UrlFormatterAttributeTests.cs" company="Microsoft Corporation">
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
    public class UrlFormatterAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertWithUriFormattingThenTextPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithUri>();

            Assert.AreEqual(3, contractsAlert.AlertProperties.Count);

            int propertyIndex = 1;
            Assert.AreEqual("LongUrlValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("LongUrlDisplayName", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(0, ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("<a href=\"https://www.bing.com/\" target=\"_blank\">LinkText Link text</a>", ((LongTextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);

            propertyIndex++;
            Assert.AreEqual("UrlValue", contractsAlert.AlertProperties[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, contractsAlert.AlertProperties[propertyIndex].Type);
            Assert.AreEqual("UrlDisplayName", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).DisplayName);
            Assert.AreEqual(1, ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Order);
            Assert.AreEqual("<a href=\"https://www.microsoft.com/\" target=\"_blank\">LinkText Link text</a>", ((TextAlertProperty)contractsAlert.AlertProperties[propertyIndex]).Value);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithUriFormattingOnNonUriPropertyThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithNonUri>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithUriFormattingOnRelativeUriPropertyThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithRelativeUri>();
        }

        public class TestAlertWithUri : TestAlertBase
        {
            public string RawProperty => "Link text";

            [UrlFormatter("LinkText {RawProperty}")]
            [LongTextProperty("LongUrlDisplayName")]
            public Uri LongUrlValue => new Uri("https://www.bing.com");

            [UrlFormatter("LinkText {RawProperty}")]
            [TextProperty("UrlDisplayName")]
            public Uri UrlValue => new Uri("https://www.microsoft.com");
        }

        public class TestAlertWithNonUri : TestAlertBase
        {
            public string RawProperty => "Link text";

            [UrlFormatter("LinkText {RawProperty}")]
            [LongTextProperty("LongUrlDisplayName")]
            public string LongValue => "Oops";
        }

        public class TestAlertWithRelativeUri : TestAlertBase
        {
            public string RawProperty => "Link text";

            [UrlFormatter("LinkText {RawProperty}")]
            [TextProperty("UrlDisplayName")]
            public Uri UrlValue => new Uri("some/path", UriKind.Relative);
        }
    }
}
