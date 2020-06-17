//-----------------------------------------------------------------------
// <copyright file="TablePropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    [TestClass]
    public class TablePropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenTablePropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            TableAlertProperty<string> singleColumnTableAlertProperty = (TableAlertProperty<string>)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("SingleColumnTable", singleColumnTableAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, singleColumnTableAlertProperty.Type);
            Assert.AreEqual("SingleColumnTableDisplayName", singleColumnTableAlertProperty.DisplayName);
            Assert.AreEqual(0, singleColumnTableAlertProperty.Order);
            Assert.AreEqual(false, singleColumnTableAlertProperty.ShowHeaders);

            Assert.AreEqual(3, singleColumnTableAlertProperty.Values.Count);
            Assert.AreEqual("value1", singleColumnTableAlertProperty.Values[0]);
            Assert.AreEqual("value2", singleColumnTableAlertProperty.Values[1]);
            Assert.AreEqual("value3", singleColumnTableAlertProperty.Values[2]);

            Assert.AreEqual(0, singleColumnTableAlertProperty.Columns.Count);

            propertyIndex++;
            TableAlertProperty<Dictionary<string, string>> multiColumnTableAlertProperty = (TableAlertProperty<Dictionary<string, string>>)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("Table", multiColumnTableAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, multiColumnTableAlertProperty.Type);
            Assert.AreEqual("TableDisplayName", multiColumnTableAlertProperty.DisplayName);
            Assert.AreEqual(1, multiColumnTableAlertProperty.Order);
            Assert.AreEqual(true, multiColumnTableAlertProperty.ShowHeaders);

            Assert.AreEqual(2, multiColumnTableAlertProperty.Values.Count);
            Assert.AreEqual(4, multiColumnTableAlertProperty.Values[0].Count);
            Assert.AreEqual("p11", multiColumnTableAlertProperty.Values[0]["Prop1"]);
            Assert.AreEqual("The value of Prop2 is p21", multiColumnTableAlertProperty.Values[0]["Prop2"]);
            Assert.AreEqual("p31", multiColumnTableAlertProperty.Values[0]["Prop3"]);
            Assert.AreEqual("<a href=\"http://microsoft.com/\">Link for NDP1</a>", multiColumnTableAlertProperty.Values[0]["UriProp"]);

            Assert.AreEqual(4, multiColumnTableAlertProperty.Values[1].Count);
            Assert.AreEqual("p12", multiColumnTableAlertProperty.Values[1]["Prop1"]);
            Assert.AreEqual("The value of Prop2 is p22", multiColumnTableAlertProperty.Values[1]["Prop2"]);
            Assert.AreEqual("p32", multiColumnTableAlertProperty.Values[1]["Prop3"]);
            Assert.AreEqual("<a href=\"http://contoso.com/\">Link for NDP2</a>", multiColumnTableAlertProperty.Values[1]["UriProp"]);

            Assert.AreEqual(4, multiColumnTableAlertProperty.Columns.Count);
            Assert.AreEqual("Prop1", multiColumnTableAlertProperty.Columns[0].PropertyName);
            Assert.AreEqual("First Prop", multiColumnTableAlertProperty.Columns[0].DisplayName);
            Assert.AreEqual("Prop2", multiColumnTableAlertProperty.Columns[1].PropertyName);
            Assert.AreEqual("Second Prop", multiColumnTableAlertProperty.Columns[1].DisplayName);
            Assert.AreEqual("UriProp", multiColumnTableAlertProperty.Columns[2].PropertyName);
            Assert.AreEqual("Uri Prop", multiColumnTableAlertProperty.Columns[2].DisplayName);
            Assert.AreEqual("Prop3", multiColumnTableAlertProperty.Columns[3].PropertyName);
            Assert.AreEqual("Third Prop, without order", multiColumnTableAlertProperty.Columns[3].DisplayName);
        }

        [TestMethod]
        public void WhenCreatingContractsAlertWithReferencePropertiesThenTablePropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferenceProperties>();

            Assert.AreEqual(2, contractsAlert.AlertProperties.Count);

            int propertyIndex = 0;
            TableReferenceAlertProperty multiColumnTableAlertProperty = (TableReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("MultiColumnTableReference", multiColumnTableAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, multiColumnTableAlertProperty.Type);
            Assert.AreEqual("MultiColumnTableReferenceDisplayName", multiColumnTableAlertProperty.DisplayName);
            Assert.AreEqual(0, multiColumnTableAlertProperty.Order);
            Assert.AreEqual(true, multiColumnTableAlertProperty.ShowHeaders);
            Assert.AreEqual("multiColumnTableReferencePath", multiColumnTableAlertProperty.ReferencePath);

            Assert.AreEqual(4, multiColumnTableAlertProperty.Columns.Count);
            Assert.AreEqual("Prop1", multiColumnTableAlertProperty.Columns[0].PropertyName);
            Assert.AreEqual("First Prop", multiColumnTableAlertProperty.Columns[0].DisplayName);
            Assert.AreEqual("Prop2", multiColumnTableAlertProperty.Columns[1].PropertyName);
            Assert.AreEqual("Second Prop", multiColumnTableAlertProperty.Columns[1].DisplayName);
            Assert.AreEqual("UriProp", multiColumnTableAlertProperty.Columns[2].PropertyName);
            Assert.AreEqual("Uri Prop", multiColumnTableAlertProperty.Columns[2].DisplayName);
            Assert.AreEqual("Prop3", multiColumnTableAlertProperty.Columns[3].PropertyName);
            Assert.AreEqual("Third Prop, without order", multiColumnTableAlertProperty.Columns[3].DisplayName);

            propertyIndex++;
            TableReferenceAlertProperty singleColumnTableAlertProperty = (TableReferenceAlertProperty)contractsAlert.AlertProperties[propertyIndex];
            Assert.AreEqual("SingleColumnTableReference", singleColumnTableAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, singleColumnTableAlertProperty.Type);
            Assert.AreEqual("SingleColumnTableReferenceDisplayName", singleColumnTableAlertProperty.DisplayName);
            Assert.AreEqual(1, singleColumnTableAlertProperty.Order);
            Assert.AreEqual(false, singleColumnTableAlertProperty.ShowHeaders);
            Assert.AreEqual("singleColumnTableReference", singleColumnTableAlertProperty.ReferencePath);
            Assert.AreEqual(0, singleColumnTableAlertProperty.Columns.Count);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithReferencePropertiesAndInvalidColumnsThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferencePropertiesAndNotReferenceTableData>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithNotGenericReferencePropertiesThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithNotGenericReferenceProperties>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithReferencePropertiesWithFormatStringThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertWithReferencePropertiesAndFormatString>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithNonListTableThenExceptionIsThrown()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlertOnNotList>();
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingContractsAlertWithMismatchListTableThenExceptionIsThrown()
        {
            var alert = new TestAlert();
            alert.SingleColumnTable.Add(2);
            ContractsAlert contractsAlert = alert.CreateContractsAlert(AnalysisRequest, "detector", false, false);
        }

        public class TestAlert : TestAlertBase
        {
            public TestAlert()
            {
                this.SingleColumnTable = new List<object> { "value1", "value2", "value3" };
            }

            [MultiColumnTableProperty("TableDisplayName", ShowHeaders = true)]
            [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Test code, allowed")]
            public TableData[] Table => new[]
            {
                new TableData { Prop1 = "p11", Prop2 = "p21", Prop3 = "p31", UriProp = new Uri("http://microsoft.com"), NonDisplayProp = "NDP1" },
                new TableData { Prop1 = "p12", Prop2 = "p22", Prop3 = "p32", UriProp = new Uri("http://contoso.com"), NonDisplayProp = "NDP2" },
            };

            [SingleColumnTableProperty("SingleColumnTableDisplayName", ShowHeaders = false)]
            public List<object> SingleColumnTable { get; }
        }

        public class TestAlertWithReferenceProperties : TestAlertBase
        {
            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", ShowHeaders = true)]
            public TablePropertyReference<ReferenceTableData> MultiColumnTableReference => new TablePropertyReference<ReferenceTableData>("multiColumnTableReferencePath");

            [SingleColumnTableProperty("SingleColumnTableReferenceDisplayName", ShowHeaders = false)]
            public PropertyReference SingleColumnTableReference => new PropertyReference("singleColumnTableReference");
        }

        public class TestAlertWithReferencePropertiesAndNotReferenceTableData : TestAlertBase
        {
            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", ShowHeaders = true)]
            public TablePropertyReference<TableData> MultiColumnTableReference => new TablePropertyReference<TableData>("multiColumnTableReferencePath");
        }

        public class TestAlertWithNotGenericReferenceProperties : TestAlertBase
        {
            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", ShowHeaders = true)]
            public PropertyReference MultiColumnTableReference => new PropertyReference("multiColumnTableReferencePath");
        }

        public class TestAlertWithReferencePropertiesAndFormatString : TestAlertBase
        {
            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", ShowHeaders = true)]
            public TablePropertyReference<RefernceTableDataWithFormatString> MultiColumnTableReference => new TablePropertyReference<RefernceTableDataWithFormatString>("multiColumnTableReferencePath");
        }

        public class TestAlertOnNotList : TestAlertBase
        {
            [SingleColumnTableProperty("SingleColumnTableDisplayName")]
            public string SingleColumnTable => "Oops";
        }

        public class TableData
        {
            [JsonProperty("prop3")]
            [TableColumn("Third Prop, without order")]
            public string Prop3 { get; set; }

            [TableColumn("Second Prop", Order = 2, FormatString = "The value of Prop2 is {0}")]
            public string Prop2 { get; set; }

            [UrlFormatter("Link for {NonDisplayProp}")]
            [TableColumn("Uri Prop", Order = 3)]
            public Uri UriProp { get; set; }

            // The properties are sorted like this to ensure that
            // the Order property is taken into account
            [JsonProperty("prop1")]
            [TableColumn("First Prop", Order = 1)]
            public string Prop1 { get; set; }

            public string NonDisplayProp { get; set; }
        }

        public class ReferenceTableData
        {
            [JsonProperty("prop3")]
            [TableColumn("Third Prop, without order")]
            public string Prop3 { get; set; }

            [TableColumn("Second Prop", Order = 2)]
            public string Prop2 { get; set; }

            [UrlFormatter("Link for {NonDisplayProp}")]
            [TableColumn("Uri Prop", Order = 3)]
            public Uri UriProp { get; set; }

            // The properties are sorted like this to ensure that
            // the Order property is taken into account
            [JsonProperty("prop1")]
            [TableColumn("First Prop", Order = 1)]
            public string Prop1 { get; set; }

            public string NonDisplayProp { get; set; }
        }

        public class RefernceTableDataWithFormatString
        {
            [TableColumn("Some Property", FormatString = "Oops")]
            public string Prop { get; set; }
        }
    }
}
