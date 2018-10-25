//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfigurationControl.xaml.cs" company="Microsoft Corporation">
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
    public partial class SmartDetectorConfigurationControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControl"/> class.
        /// </summary>
        public SmartDetectorConfigurationControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container.Resolve<SmartDetectorConfigurationControlViewModel>();
        }
    }
}
