//-----------------------------------------------------------------------
// <copyright file="TextPropertyControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// Interaction logic for TextPropertyControl.xaml
    /// </summary>
    public partial class TextPropertyControl : UserControl
    {
        /// <summary>
        /// The alert text property that should be displayed.
        /// </summary>
        public static readonly DependencyProperty TextAlertPropertyProperty = DependencyProperty.Register(
            "TextAlertProperty",
            typeof(TextAlertProperty),
            typeof(TextPropertyControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPropertyControl"/> class.
        /// </summary>
        public TextPropertyControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the alert text property.
        /// </summary>
        public TextAlertProperty TextAlertProperty
        {
            get
            {
                return (TextAlertProperty)this.GetValue(TextAlertPropertyProperty);
            }

            set
            {
                this.SetValue(TextAlertPropertyProperty, value);
            }
        }
    }
}
