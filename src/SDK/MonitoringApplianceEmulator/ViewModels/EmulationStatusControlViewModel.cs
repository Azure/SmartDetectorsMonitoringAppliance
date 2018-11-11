//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="EmulationStatusControl"/> control.
    /// </summary>
    public class EmulationStatusControlViewModel : ObservableObject
    {
        private readonly NotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStatusControlViewModel"/> class for design time only.
        /// </summary>
        public EmulationStatusControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStatusControlViewModel"/> class.
        /// </summary>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        /// <param name="notificationService">The notification service.</param>
        [InjectionConstructor]
        public EmulationStatusControlViewModel(IEmulationSmartDetectorRunner smartDetectorRunner, NotificationService notificationService)
        {
            this.SmartDetectorRunner = smartDetectorRunner;
            this.notificationService = notificationService;
        }

        #region Binded Properties

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public IEmulationSmartDetectorRunner SmartDetectorRunner { get; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that switch to the Alerts Control tab
        /// </summary>
        public CommandHandler SwitchTabCommand => new CommandHandler(() => this.notificationService.OnTabSwitchedToAlertsControl());

        #endregion
    }
}
