//-----------------------------------------------------------------------
// <copyright file="AlertsRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Alerts repository model. Holds all Alerts created in the current run.
    /// </summary>
    public class AlertsRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsRepository"/> class.
        /// </summary>
        public AlertsRepository()
        {
            this.Alerts = new ObservableCollection<List<EmulationAlert>>();
        }

        /// <summary>
        /// Gets the collection of Alerts in the repository.
        /// </summary>
        public ObservableCollection<List<EmulationAlert>> Alerts { get; }
    }
}
