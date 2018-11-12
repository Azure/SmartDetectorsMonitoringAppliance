//-----------------------------------------------------------------------
// <copyright file="AlertsControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System.Windows.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Unity;

    /// <summary>
    /// Interaction logic for AlertsControl.xaml
    /// </summary>
    public partial class AlertsControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsControl"/> class.
        /// </summary>
        public AlertsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container?.Resolve<AlertsControlViewModel>() ?? new AlertsControlViewModel();
        }
    }
}
