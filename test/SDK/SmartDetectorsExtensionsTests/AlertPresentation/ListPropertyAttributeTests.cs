//-----------------------------------------------------------------------
// <copyright file="ListPropertyAttributeTests.cs" company="Microsoft Corporation">
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
    public class ListPropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenMetricListPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(11, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            this.VerifyRawAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_0_RawProperty", "raw");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_0_Name1", "First name title", 0, "First name");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_0_Uri1", "First link", 1, "<a href=\"https://xkcd.com/\" target=\"_blank\">Link to data 1</a>");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_Name2", "Second name title", 2, "Second name");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_Uri2", "Second link", 3, "<a href=\"https://darwinawards.com/\" target=\"_blank\">Link to data 2</a>");
            this.VerifyRawAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_0_RawProperty", "raw");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_0_Name1", "First name title", 4, "First name");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_0_Uri1", "First link", 5, "<a href=\"https://xkcd.com/\" target=\"_blank\">Link to data 1</a>");
            this.VerifyRawAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_1_RawProperty", "raw");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_1_Name1", "First name title", 6, "First name");
            this.VerifyTextAlertProperty(contractsAlert.AlertProperties[propertyIndex++], "AdditionalData_1_MoreData_1_Uri1", "First link", 7, "<a href=\"https://xkcd.com/\" target=\"_blank\">Link to data 1</a>");
        }

        private void VerifyTextAlertProperty(AlertProperty alertProperty, string propertyName, string displayName, byte order, string value)
        {
            Assert.AreEqual(propertyName, alertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, alertProperty.Type);
            Assert.AreEqual(displayName, ((TextAlertProperty)alertProperty).DisplayName);
            Assert.AreEqual(order, ((TextAlertProperty)alertProperty).Order);
            Assert.AreEqual(value, ((TextAlertProperty)alertProperty).Value);
        }

        private void VerifyRawAlertProperty(AlertProperty alertProperty, string propertyName, object value)
        {
            Assert.AreEqual(propertyName, alertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Raw, alertProperty.Type);
            Assert.AreEqual(value, ((RawAlertProperty)alertProperty).Value);
        }

        public class TestAlert : TestAlertBase
        {
            [ListProperty]
            public IList<object> AdditionalData => new List<object>()
            {
                new ListData1(),
                new ListData2()
            };
        }

        private class ListData1
        {
            [TextProperty("First name title", Order = 1)]
            public string Name1 => "First name";

            [UrlFormatter("Link to data 1")]
            [TextProperty("First link", Order = 2)]
            public Uri Uri1 => new Uri("https://xkcd.com/");

            public string RawProperty => "raw";
        }

        private class ListData2
        {
            [TextProperty("Second name title", Order = 1)]
            public string Name2 => "Second name";

            [UrlFormatter("Link to data 2")]
            [TextProperty("Second link", Order = 2)]
            public Uri Uri2 => new Uri("https://darwinawards.com/");

            [ListProperty(Order = 3)]
            public IList<ListData1> MoreData => new List<ListData1>()
            {
                new ListData1(),
                new ListData1()
            };

            [ListProperty(Order = 4)]
            public IList<ListData1> EmptyList => new List<ListData1>();
        }
    }
}
