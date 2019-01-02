//-----------------------------------------------------------------------
// <copyright file="TablePropertyControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The view model class for the <see cref="TablePropertyControlViewModel{T}"/> control.
    /// </summary>
    /// <typeparam name="T">The row type of the table</typeparam>
    public class TablePropertyControlViewModel<T> : ObservableObject
    {
        private string title;
        private DataTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="TablePropertyControlViewModel{T}"/> class.
        /// </summary>
        /// <param name="tableAlertProperty">The table alert property that should be displayed.</param>
        public TablePropertyControlViewModel(TableAlertProperty<T> tableAlertProperty)
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
                Dictionary<string, string> valueAsDictionary;
                if (value.GetType() == typeof(Dictionary<string, string>))
                {
                    valueAsDictionary = value as Dictionary<string, string>;
                }
                else
                {
                    var valueAsJToken = JToken.FromObject(value);
                    valueAsDictionary = valueAsJToken.ToObject<Dictionary<string, string>>();
                }

                var newRow = table.NewRow();
                foreach (var tableColumn in tableAlertProperty.Columns)
                {
                    string valueAsString = string.Empty;
                    valueAsDictionary?.TryGetValue(tableColumn.PropertyName, out valueAsString);
                    newRow[tableColumn.DisplayName] = valueAsString;
                }

                table.Rows.Add(newRow);
            }

            this.Title = tableAlertProperty.DisplayName;
            this.Table = table;
        }

        /// <summary>
        /// Gets the chart title.
        /// </summary>
        public string Title
        {
            get => this.title;

            private set
            {
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        public DataTable Table
        {
            get => this.table;

            private set
            {
                this.table = value;
                this.OnPropertyChanged();
            }
        }
    }
}