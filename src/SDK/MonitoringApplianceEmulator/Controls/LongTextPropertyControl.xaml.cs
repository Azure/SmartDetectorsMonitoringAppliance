//-----------------------------------------------------------------------
// <copyright file="LongTextPropertyControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// Interaction logic for LongTextPropertyControl.xaml
    /// </summary>
    public partial class LongTextPropertyControl : UserControl
    {
        /// <summary>
        /// The alert text property that should be displayed.
        /// </summary>
        public static readonly DependencyProperty LongTextAlertPropertyProperty = DependencyProperty.Register(
            "LongTextAlertProprety",
            typeof(LongTextAlertProprety),
            typeof(LongTextPropertyControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="LongTextPropertyControl"/> class.
        /// </summary>
        public LongTextPropertyControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the alert long text property.
        /// </summary>
        public LongTextAlertProprety LongTextAlertProprety
        {
            get
            {
                return (LongTextAlertProprety)this.GetValue(LongTextAlertPropertyProperty);
            }

            set
            {
                this.SetValue(LongTextAlertPropertyProperty, value);
            }
        }
    }
}
