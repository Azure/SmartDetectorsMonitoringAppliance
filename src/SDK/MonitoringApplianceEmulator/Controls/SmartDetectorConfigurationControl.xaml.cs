//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfigurationControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Unity;

    /// <summary>
    /// Interaction logic for AlertsControl.xaml
    /// </summary>
    public partial class SmartDetectorConfigurationControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControl"/> class.
        /// </summary>
        public SmartDetectorConfigurationControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container?.Resolve<SmartDetectorConfigurationControlViewModel>() ?? new SmartDetectorConfigurationControlViewModel();
        }

        private void OnSubscriptionComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Get target combo box and make it searchable
            var targetComboBox = sender as ComboBox;
            targetComboBox?.MakeSearchable(item =>
            {
                if (item is HierarchicalResource resource)
                {
                    return resource.Name;
                }

                return item.ToString();
            });
        }
    }
}
