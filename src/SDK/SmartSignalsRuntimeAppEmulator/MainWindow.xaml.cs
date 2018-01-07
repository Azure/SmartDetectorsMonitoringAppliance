//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Unity;

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels;
    using Unity.Attributes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = App.Container.Resolve<MainWindowViewModel>();
        }

        #region Overrides of Window

        /// <summary>
        /// Runs after initialization and removes the icon from the title bar.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IconHelper.RemoveIcon(this);
        }

        #endregion
    }
}
