//-----------------------------------------------------------------------
// <copyright file="KeyValuePropertyControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using RuntimeEnvironment.Contracts;

    /// <summary>
    /// Interaction logic for KeyValuePropertyControl.xaml
    /// </summary>
    public partial class KeyValuePropertyControl : UserControl
    {
        /// <summary>
        /// The text alert property that should be displayed.
        /// </summary>
        public static readonly DependencyProperty KeyValueAlertPropertyProperty = DependencyProperty.Register(
            "KeyValueAlertProperty",
            typeof(KeyValueAlertProperty),
            typeof(KeyValuePropertyControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePropertyControl"/> class.
        /// </summary>
        public KeyValuePropertyControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the alert key value property.
        /// </summary>
        public KeyValueAlertProperty KeyValueAlertProperty
        {
            get
            {
                return (KeyValueAlertProperty)this.GetValue(KeyValueAlertPropertyProperty);
            }

            set
            {
                this.SetValue(KeyValueAlertPropertyProperty, value);
            }
        }
    }
}
