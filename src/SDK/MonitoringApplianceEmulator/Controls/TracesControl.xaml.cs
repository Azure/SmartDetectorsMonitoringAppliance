//-----------------------------------------------------------------------
// <copyright file="TracesControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
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
    }
}
