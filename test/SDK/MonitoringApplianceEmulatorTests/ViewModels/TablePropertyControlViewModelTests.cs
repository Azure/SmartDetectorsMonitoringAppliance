//-----------------------------------------------------------------------
// <copyright file="TablePropertyControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class TablePropertyControlViewModelTests
    {
        [TestMethod]
        public void WhenCreatingNewViewModelThenItWasInitializedCorrectly()
        {
            var columns = new List<TableColumn>()
            {
                new TableColumn(nameof(TestTableAlertPropertyValue.FirstName), "First Name"),
                new TableColumn(nameof(TestTableAlertPropertyValue.LastName), "Last Name"),
                new TableColumn(nameof(TestTableAlertPropertyValue.Goals), "Goals avg")
            };

            var rows = new List<TestTableAlertPropertyValue>()
            {
                new TestTableAlertPropertyValue() { FirstName = "Edinson", LastName = "Cavani", Goals = 4.67 },
                new TestTableAlertPropertyValue() { FirstName = "Fernando", LastName = "Torres", Goals = 1.7 }
            };

            var tableAlertProperty = new TableAlertProperty("propertyName", "displayName", 5, true, columns, rows);

            var tablePropertyControlViewModel = new TablePropertyControlViewModel(tableAlertProperty);

            DataTable generateDataTable = tablePropertyControlViewModel.Table;

            Assert.IsNotNull(generateDataTable);

            // Verify generated table coloumns
            Assert.AreEqual(columns.Count, generateDataTable.Columns.Count, "The generated table has unexpected number of columns");
            for (int i = 0; i < columns.Count; i++)
            {
                Assert.AreEqual(columns[i].DisplayName, generateDataTable.Columns[i].ColumnName, $"The generated table's column in index {i} has unexpected name");
            }

            // Verify generated table rows values
            for (int i = 0; i < rows.Count; i++)
            {
                Assert.AreEqual(rows[i].FirstName, generateDataTable.Rows[i]["First Name"], $"Unexpected value in row: {i}, column: 'First Name'");
                Assert.AreEqual(rows[i].LastName, generateDataTable.Rows[i]["Last Name"], $"Unexpected value in row: {i}, column: 'Last Name'");
                Assert.AreEqual(rows[i].Goals.ToString("G", CultureInfo.InvariantCulture), generateDataTable.Rows[i]["Goals avg"], $"Unexpected value in row: {i}, column: 'Goals avg'");
            }
        }
    }
}