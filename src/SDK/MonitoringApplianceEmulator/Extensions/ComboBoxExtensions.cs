//-----------------------------------------------------------------------
// <copyright file="ComboBoxExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Extensions
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Extension methods for <see cref="ComboBox"/> objects.
    /// </summary>
    public static class ComboBoxExtensions
    {
        /// <summary>
        /// Make the combo box searchable.
        /// This method is typically called by the OnLoaded combo box event handler.
        /// </summary>
        /// <param name="targetComboBox">The target combo box</param>
        /// <param name="getItemText">A function that returns the text to search for each combo box item</param>
        public static void MakeSearchable(this ComboBox targetComboBox, Func<object, string> getItemText)
        {
            const string TextInputState = "TextInput";
            const string SelectionState = "Selection";

            // Get target combo box and text box
            var targetTextBox = targetComboBox?.Template.FindName("PART_EditableTextBox", targetComboBox) as TextBox;
            if (targetTextBox == null)
            {
                return;
            }

            // Set some combo box properties
            targetComboBox.Tag = TextInputState;
            targetComboBox.StaysOpenOnEdit = true;
            targetComboBox.IsEditable = true;
            targetComboBox.IsTextSearchEnabled = false;

            // When the text changes, display only the items that match the text
            targetTextBox.TextChanged += (o, args) =>
            {
                var textBox = o as TextBox;
                if (textBox == null)
                {
                    return;
                }

                string searchText = textBox.Text;

                if (targetComboBox.Tag.ToString() == SelectionState)
                {
                    // An item was just selected - show all items
                    targetComboBox.Tag = TextInputState;
                    targetComboBox.Items.Filter = item => true;
                }
                else
                {
                    if (targetComboBox.SelectionBoxItem != null)
                    {
                        // Clear the selected item
                        targetComboBox.SelectedItem = null;
                    }

                    if (string.IsNullOrEmpty(searchText))
                    {
                        // Empty string - show all items
                        targetComboBox.Items.Filter = item => true;
                        targetComboBox.SelectedItem = default(object);
                    }
                    else
                    {
                        // Show only the items that include the text
                        targetComboBox.Items.Filter = item =>
                        {
                            string itemText = getItemText(item) ?? string.Empty;
                            return itemText.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
                        };
                    }

                    // Focus on the text box
                    Keyboard.ClearFocus();
                    Keyboard.Focus(targetTextBox);

                    // Ensure the combo box is open
                    targetComboBox.IsDropDownOpen = true;

                    // Set the caret position
                    targetTextBox.SelectionStart = targetTextBox.Text.Length;
                }
            };

            // When selection changes, display all items
            targetComboBox.SelectionChanged += (o, args) =>
            {
                var comboBox = o as ComboBox;
                if (comboBox?.SelectedItem == null)
                {
                    return;
                }

                // Set selection state
                comboBox.Tag = SelectionState;
            };
        }
    }
}