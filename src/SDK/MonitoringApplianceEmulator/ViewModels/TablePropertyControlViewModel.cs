//-----------------------------------------------------------------------
// <copyright file="TablePropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System.Data;
    using System.Windows.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// The view model class for the <see cref="TablePropertyControlViewModel"/> control.
    /// </summary>
    public class TablePropertyControlViewModel : ObservableObject
    {
        private DataTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="TablePropertyControlViewModel"/> class.
        /// </summary>
        /// <param name="tableAlertProperty">The table alert property that should be displayed.</param>
        public TablePropertyControlViewModel(TableAlertProperty tableAlertProperty)
        {
            // Generate a table for the given table alert property
            var table = new DataTable();

            foreach (var column in tableAlertProperty.Columns)
            {
                var dataColumn = new DataColumn(column.DisplayName);
                table.Columns.Add(dataColumn);
            }

            foreach (object value in tableAlertProperty.Values)
            {
                var newRow = table.NewRow();
                foreach (var tableColumn in tableAlertProperty.Columns)
                {
                    newRow[tableColumn.DisplayName] = value.GetType().GetProperty(tableColumn.PropertyName)?.GetValue(value, null)?.ToString();
                }

                table.Rows.Add(newRow);
            }

            this.Table = table;
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        public DataTable Table
        {
            get
            {
                return this.table;
            }

            private set
            {
                this.table = value;
                this.OnPropertyChanged();
            }
        }
    }
}