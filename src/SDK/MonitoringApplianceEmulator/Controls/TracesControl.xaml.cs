//-----------------------------------------------------------------------
// <copyright file="TracesControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Unity;

    /// <summary>
    /// Interaction logic for TracesControl.xaml
    /// </summary>
    public partial class TracesControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TracesControl"/> class.
        /// </summary>
        public TracesControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container?.Resolve<TracesControlViewModel>() ?? new TracesControlViewModel();
            ((INotifyCollectionChanged)this.TracesGrid.Items).CollectionChanged += (sender, args) =>
            {
                if (this.TracesGrid.Items.Count > 0)
                {
                    this.TracesGrid.ScrollIntoView(this.TracesGrid.Items[this.TracesGrid.Items.Count - 1]);
                }
            };
        }

        /// <summary>
        /// Handler for the PreviewTextInput event - we use this event to accept only digit inputs from the user
        /// </summary>
        /// <param name="sender">The event sender (e.g. text box)</param>
        /// <param name="e">The event args</param>
        private void CurrentPageTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        /// <summary>
        /// Handler for the KeyUp event - we use this event to force binding update when the user has pressed Enter key
        /// </summary>
        /// <param name="sender">The event sender (e.g. text box)</param>
        /// <param name="e">The event args</param>
        private void CurrentPageTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;

                BindingExpression binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                binding?.UpdateSource();
            }
        }
    }
}
