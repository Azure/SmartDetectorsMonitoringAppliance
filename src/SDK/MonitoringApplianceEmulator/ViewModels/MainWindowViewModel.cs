//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="MainWindow"/> control.
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        private IEmulationSmartDetectorRunner smartDetectorRunner;
        private IAuthenticationServices authenticationServices;
        private MainWindowTabItem selectedTab = MainWindowTabItem.SmartDetectorConfigurationControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class for design time only.
        /// </summary>
        public MainWindowViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services to use.</param>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        /// <param name="notificationService">The notification service.</param>
        [InjectionConstructor]
        public MainWindowViewModel(IAuthenticationServices authenticationServices, IEmulationSmartDetectorRunner smartDetectorRunner, NotificationService notificationService)
        {
            this.AuthenticationServices = authenticationServices;
            this.SmartDetectorRunner = smartDetectorRunner;
            notificationService.TabSwitchedToAlertsControl += () => { this.SelectedTab = MainWindowTabItem.AlertsControl; };
        }

        /// <summary>
        /// Gets or sets the selected Tab
        /// </summary>
        public MainWindowTabItem SelectedTab
        {
            get => this.selectedTab;

            set
            {
                this.selectedTab = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public IEmulationSmartDetectorRunner SmartDetectorRunner
        {
            get
            {
                return this.smartDetectorRunner;
            }

            private set
            {
                this.smartDetectorRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the name of the signed in user.
        /// </summary>
        public IAuthenticationServices AuthenticationServices
        {
            get => this.authenticationServices;

            private set
            {
                this.authenticationServices = value;
                this.OnPropertyChanged();
            }
        }
    }
}
