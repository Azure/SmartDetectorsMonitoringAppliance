//-----------------------------------------------------------------------
// <copyright file="SignalsControl.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels;
    using Unity;    

    /// <summary>
    /// Interaction logic for SignalsControl.xaml
    /// </summary>
    public partial class SignalsControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsControl"/> class.
        /// </summary>
        public SignalsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Container.Resolve<SignalsControlViewModel>();
        }

        /// <summary>
        /// Temporary - start analysis.
        /// </summary>
        /// <param name="sender">The element that was clicked</param>
        /// <param name="e">The event args</param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.BeforeStartMessage.Visibility = Visibility.Collapsed;
            this.EmulationStatusControl.Visibility = Visibility.Visible;
        }
    }
}
