//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerRequestPropertyAttributeTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartAxisType;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    [TestClass]
    public class AzureResourceManagerRequestPropertyAttributeTests : PresentationAttributeTestsBase
    {
        [TestMethod]
        public void WhenCreatingContractsAlertThenArmRequestPropertiesAreConvertedCorrectly()
        {
            ContractsAlert contractsAlert = CreateContractsAlert<TestAlert>();

            Assert.AreEqual(1, contractsAlert.AlertProperties.Count);

            AzureResourceManagerRequestAlertProperty armAlertProperty = (AzureResourceManagerRequestAlertProperty)contractsAlert.AlertProperties[0];
            Assert.AreEqual("ArmRequest", armAlertProperty.PropertyName);
            Assert.AreEqual(AlertPropertyType.AzureResourceManagerRequest, armAlertProperty.Type);
            Assert.AreEqual("/some/query/path", armAlertProperty.AzureResourceManagerRequestUri.ToString());
            Assert.AreEqual(6, armAlertProperty.PropertiesToDisplay.Count);

            int propertyIndex = 0;
            Assert.AreEqual("TextReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Text, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("LongTextReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.LongText, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("KeyValueReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.KeyValue, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("ChartReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Chart, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("MultiColumnTableReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);

            propertyIndex++;
            Assert.AreEqual("SingleColumnTableReference", armAlertProperty.PropertiesToDisplay[propertyIndex].PropertyName);
            Assert.AreEqual(AlertPropertyType.Table, armAlertProperty.PropertiesToDisplay[propertyIndex].Type);
        }

        public class TestAlert : TestAlertBase
        {
            [AzureResourceManagerRequestProperty]
            public ArmRequestWithDisplay ArmRequest => new ArmRequestWithDisplay(new Uri("/some/query/path", UriKind.Relative));
        }

        public class ArmRequestWithDisplay : AzureResourceManagerRequest
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ArmRequestWithDisplay"/> class.
            /// </summary>
            /// <param name="requestUri">The request's URI. This must be a relative URI that will be executed against the ARM endpoint.</param>
            public ArmRequestWithDisplay(Uri requestUri)
                : base(requestUri)
            {
            }

            [TextProperty("TextReferenceDisplayName", Order = 1)]
            public PropertyReference TextReference => new PropertyReference("textReferencePath");

            [LongTextProperty("LongTextReferenceDisplayName", Order = 2)]
            public PropertyReference LongTextReference => new PropertyReference("longTextReferencePath");

            [KeyValueProperty("KeyValueReferenceDisplayName", Order = 3)]
            public PropertyReference KeyValueReference => new PropertyReference("keyValueReferencePath");

            [ChartProperty("ChartReferenceDisplayName", ChartType.LineChart, ChartAxisType.DateAxis, ChartAxisType.NumberAxis, Order = 4)]
            public PropertyReference ChartReference => new PropertyReference("chartReferencePath");

            [MultiColumnTableProperty("MultiColumnTableReferenceDisplayName", Order = 5, ShowHeaders = true)]
            public TablePropertyReference<ReferenceTableData> MultiColumnTableReference => new TablePropertyReference<ReferenceTableData>("multiColumnTableReferencePath");

            [SingleColumnTableProperty("SingleColumnTableReferenceDisplayName", Order = 6, ShowHeaders = false)]
            public PropertyReference SingleColumnTableReference => new PropertyReference("singleColumnTableReference");
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
    }
}
