//-----------------------------------------------------------------------
// <copyright file="AlertsControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// Occurs when the user closed the alert details control.
    /// </summary>
    public delegate void AlertDetailsControlClosedEventHandler();

    /// <summary>
    /// The view model class for the <see cref="AlertsControl"/> control.
    /// </summary>
    public class AlertsControlViewModel : ObservableObject
    {
        private readonly ISystemProcessClient systemProcessClient;

        private IEmulationSmartDetectorRunner smartDetectorRunner;

        private EmulationAlert selectedAlert;

        private AlertDetailsControlViewModel alertDetailsControlViewModel;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsControlViewModel"/> class for design time only.
        /// </summary>
        public AlertsControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsControlViewModel"/> class.
        /// </summary>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        /// <param name="systemProcessClient">The system process client.</param>
        [InjectionConstructor]
        public AlertsControlViewModel(IEmulationSmartDetectorRunner smartDetectorRunner, ISystemProcessClient systemProcessClient)
        {
            this.SmartDetectorRunner = smartDetectorRunner;
            this.SelectedAlert = null;
            this.AlertDetailsControlViewModel = null;
            this.systemProcessClient = systemProcessClient;

            this.AlertDetailsControlClosed += () =>
            {
                this.SelectedAlert = null;
            };
        }

        #endregion

        /// <summary>
        /// Handler for closing the alert details control event.
        /// </summary>
        public event AlertDetailsControlClosedEventHandler AlertDetailsControlClosed;

        #region Binded Properties

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
        /// Gets or sets the selected alert.
        /// </summary>
        public EmulationAlert SelectedAlert
        {
            get
            {
                return this.selectedAlert;
            }

            set
            {
                this.selectedAlert = value;
                this.OnPropertyChanged();

                if (this.selectedAlert != null)
                {
                    this.AlertDetailsControlViewModel = new AlertDetailsControlViewModel(this.selectedAlert, this.AlertDetailsControlClosed, this.systemProcessClient);
                }
                else
                {
                    this.AlertDetailsControlViewModel = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected alert details control view model.
        /// </summary>
        public AlertDetailsControlViewModel AlertDetailsControlViewModel
        {
            get
            {
                return this.alertDetailsControlViewModel;
            }

            set
            {
                this.alertDetailsControlViewModel = value;
                this.OnPropertyChanged();
            }
        }

        #endregion
    }
}
