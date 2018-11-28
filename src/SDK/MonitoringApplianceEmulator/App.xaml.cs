//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Resources.NeutralResourcesLanguage("en")]
namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using IdentityModel.Clients.ActiveDirectory;
    using KeyVault;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.Win32;
    using Unity;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string TempSubFolderName = "MonitoringApplianceEmulatorTemp";

        private static string tempFolder;

        /// <summary>
        /// Gets the unity container.
        /// </summary>
        public static IUnityContainer Container { get; private set; }

        /// <summary>
        /// This method is being invoked when the application is about to exit.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The exit event arguments</param>
        public void OnExit(object sender, ExitEventArgs e)
        {
            Container.Resolve<UserSettings>().Save();
        }

        /// <summary>
        /// Raises the <see cref="Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Cleanup previous temp folders (that are at least 2 days old), and create a new temp folder
            FileSystemExtensions.CleanupTempFolders(TempSubFolderName, 48);
            tempFolder = FileSystemExtensions.CreateTempFolder(TempSubFolderName);

            NotificationService notificationService = new NotificationService();
            IExtendedTracer consoleTracer = new ConsoleTracer(string.Empty);
            var smartDetectorLoader = new SmartDetectorLoader(tempFolder, consoleTracer);

            // *Temporary*: if package file path wasn't accepted, raise file selection window to allow package file selection.
            // This option should be removed before launching version for customers (bug for tracking: 1177247)
            string smartDetectorPackagePath = e.Args.Length < 1 ?
                GetSmartDetectorPackagePath() :
                Diagnostics.EnsureStringNotNullOrWhiteSpace(() => e.Args[0]);

            SmartDetectorPackage smartDetectorPackage;
            using (var fileStream = new FileStream(smartDetectorPackagePath, FileMode.Open))
            {
                smartDetectorPackage = SmartDetectorPackage.CreateFromStream(fileStream);
            }

            try
            {
                SmartDetectorManifest smartDetectorManifest = smartDetectorPackage.Manifest;
                ISmartDetector detector = smartDetectorLoader.LoadSmartDetector(smartDetectorPackage);

                // Authenticate the user to Active Directory
                IAuthenticationServices authenticationServices;
                string dirId = null;
                if (e.Args.Length == 2)
                {
                    var subId = e.Args[1];
                    dirId = GetDirectoryForSubscriptionAsync(subId).Result;
                    var kv = new KeyVaultClient(GetToken);
                    var sec = kv.GetSecretAsync("https://cadservicepreviewvault.vault.azure.net/secrets/CadServicePreview/836cea07a62147fd9d614a8dd0ac1e91").Result;
                    Console.WriteLine(sec.Value);
                    authenticationServices = new AuthenticationServices(dirId, "b6836b10-1302-4894-9421-423edb2816f9", sec.Value);
                }
                else
                {
                    authenticationServices = new AuthenticationServices();
                }

                authenticationServices.AuthenticateUser();
                ICredentialsFactory credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices);
                IHttpClientWrapper httpClientWrapper = new HttpClientWrapper();
                IExtendedAzureResourceManagerClient azureResourceManagerClient = new ExtendedAzureResourceManagerClient(httpClientWrapper, credentialsFactory, consoleTracer);

                // Create analysis service factory
                var queryRunInfoProvider = new QueryRunInfoProvider(azureResourceManagerClient);
                IInternalAnalysisServicesFactory analysisServicesFactory = new AnalysisServicesFactory(consoleTracer, httpClientWrapper, credentialsFactory, azureResourceManagerClient, queryRunInfoProvider);

                // Create state repository factory
                IStateRepositoryFactory stateRepositoryFactory = new EmulationStateRepositoryFactory();

                // Load user settings
                var userSettings = UserSettings.LoadUserSettings();

                // Create the detector runner
                IPageableLogArchive logArchive = new PageableLogArchive(smartDetectorManifest.Name);
                IEmulationSmartDetectorRunner smartDetectorRunner = new SmartDetectorRunner(
                    detector,
                    analysisServicesFactory,
                    queryRunInfoProvider,
                    smartDetectorManifest,
                    stateRepositoryFactory,
                    azureResourceManagerClient,
                    logArchive);

                // Create a Unity container with all the required models and view models registrations
                Container = new UnityContainer();
                Container
                    .RegisterInstance(notificationService)
                    .RegisterInstance<ITracer>(consoleTracer)
                    .RegisterInstance(new AlertsRepository())
                    .RegisterInstance(authenticationServices)
                    .RegisterInstance(azureResourceManagerClient)
                    .RegisterInstance(detector)
                    .RegisterInstance(smartDetectorManifest)
                    .RegisterInstance(analysisServicesFactory)
                    .RegisterInstance(logArchive)
                    .RegisterInstance(smartDetectorRunner)
                    .RegisterInstance(stateRepositoryFactory)
                    .RegisterInstance(userSettings);
            }
            catch (Exception exception)
            {
                var message = $"{exception.Message}. {Environment.NewLine}{exception.InnerException?.Message}";
                MessageBox.Show(message);
                System.Diagnostics.Trace.WriteLine(message);
                Environment.Exit(1);
            }
        }

        private static Task<string> GetToken(string authority, string resource, string scope)
        {
            var authenticationContext = new AuthenticationContext(authority);
            var authenticationResult = authenticationContext.AcquireToken(
                resource,
                "1950a258-227b-4e31-a9cf-717495945fc2",
                new Uri(@"urn:ietf:wg:oauth:2.0:oob"));

            return Task.FromResult(authenticationResult.AccessToken);
        }

        /// <summary>
        /// Raises file selection dialog window to allow the user to select package file.
        /// </summary>
        /// <returns>The selected package file path or null if no file was selected</returns>
        private static string GetSmartDetectorPackagePath()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Give a subscription gets the directory id
        /// </summary>
        /// <param name="subscriptionId">The sub id</param>
        /// <returns>The directory id</returns>
        private static async Task<string> GetDirectoryForSubscriptionAsync(string subscriptionId)
        {
            string directoryId = null;

            string url = $"https://management.azure.com/subscriptions/{subscriptionId}?api-version=2014-04-01";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Method = "GET";
            try
            {
                await request.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    string authUrl = ex.Response.Headers["WWW-Authenticate"].Split(',')[0].Split('=')[1];
                    directoryId = authUrl.Substring(authUrl.LastIndexOf('/') + 1, 36);
                }
            }

            return directoryId;
        }
    }
}
