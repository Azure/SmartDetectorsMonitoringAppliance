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
        public static IUnityContainer Container { get; private set; }

        //public static IUnityContainer Container { get; private set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a Unity container with all the required models and view models registrations
            Container = new UnityContainer();
            Container
                .RegisterInstance(new SignalsResultsRepository())
                .RegisterInstance(new AuthenticationServices());

            // Authenticate the user to AAD
            var authResult = Container.Resolve<AuthenticationServices>().AuthenticateUser();

            ////var authResult = authenticationServices.AuthenticationResult;
            //var activeDirectoryToken = new ActiveDirectoryCredentials(/*authResult.AccessToken*/  "token");

            //var armClient = new AzureResourceManagerClient(activeDirectoryToken);

            //var subsIds = armClient.GetAllSubscriptionIdsAsync().GetAwaiter().GetResult();
        }
    }
}
