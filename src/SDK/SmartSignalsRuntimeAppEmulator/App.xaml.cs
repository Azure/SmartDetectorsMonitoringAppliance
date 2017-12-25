//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Unity;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a Unity container with all the required models and view models registrations
            IUnityContainer container = new UnityContainer();
            container
                .RegisterInstance(new DetectionsRepository())
                .RegisterInstance(new AuthenticationServices());

            // Authenticate the user to AAD
            container.Resolve<AuthenticationServices>().AuthenticateUserAsync();

            // Create and show the main window
            MainWindow mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}
