//-----------------------------------------------------------------------
// <copyright file="NotificationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    /// <summary>
    /// Occurs when the user switching tab from nested view.
    /// </summary>
    public delegate void SwitchTabEventHandler();

    /// <summary>
    /// Providing notification service. Can be used to Invoke and Subscribe to events.
    /// </summary>
    public class NotificationService
    {
        /// <summary>
        /// Occurs when the user switched to the Alerts Control tab from nested view
        /// </summary>
        public event SwitchTabEventHandler TabSwitchedToAlertsControl;

        /// <summary>
        /// Helper method to invoke the request of switching to the Alerts Control tab
        /// </summary>
        public void OnTabSwitchedToAlertsControl()
        {
            this.TabSwitchedToAlertsControl?.Invoke();
        }
    }
}
