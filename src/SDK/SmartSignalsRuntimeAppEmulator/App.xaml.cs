//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureResourceManagerClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Unity;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the unity container.
        /// </summary>
        public static IUnityContainer Container { get; private set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a Unity container with all the required models and view models registrations
            Container = new UnityContainer();

            // Authenticate the user to AAD
            var authenticationServices = new AuthenticationServices();
            authenticationServices.AuthenticateUser();
            var credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices.AuthenticationResult.AccessToken);
            var tracer = new ConsoleTracer(string.Empty);

            Container
                .RegisterInstance(new SignalsResultsRepository())
                .RegisterInstance(authenticationServices)
                .RegisterInstance(new AzureResourceManagerClient(credentialsFactory, tracer));
        }
    }
}
