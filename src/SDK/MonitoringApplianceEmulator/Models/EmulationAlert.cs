//-----------------------------------------------------------------------
// <copyright file="EmulationAlert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    /// <summary>
    /// Wrapper for <see cref="ContractsAlert"/> with additional emulation details.
    /// </summary>
    public class EmulationAlert : ObservableObject
    {
        private ContractsAlert contractsAlert;

        private ResourceIdentifier resourceIdentifier;

        private DateTime emulationIterationDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationAlert"/> class
        /// </summary>
        /// <param name="contractsAlert">The alert presentation object</param>
        /// <param name="emulationIterationDate">The timestamp of the emulation iteration</param>
        public EmulationAlert(ContractsAlert contractsAlert, DateTime emulationIterationDate)
        {
            this.ContractsAlert = contractsAlert;
            this.ResourceIdentifier = ResourceIdentifier.CreateFromResourceId(contractsAlert.ResourceId);
            this.EmulationIterationDate = emulationIterationDate;
        }

        /// <summary>
        /// Gets or sets the alert presentation.
        /// </summary>
        public ContractsAlert ContractsAlert
        {
            get
            {
                return this.contractsAlert;
            }

            set
            {
                this.contractsAlert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the alert's resource identifier.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier
        {
            get
            {
                return this.resourceIdentifier;
            }

            set
            {
                this.resourceIdentifier = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the timestamp of the emulation iteration.
        /// </summary>
        public DateTime EmulationIterationDate
        {
            get
            {
                return this.emulationIterationDate;
            }

            set
            {
                this.emulationIterationDate = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the alert's severity.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used for binding")]
        public string Severity => "SEV 3";

        /// <summary>
        /// Gets the alert's type.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used for binding")]
        public string Type => "Smart Detector";

        /// <summary>
        /// Gets the alert's status.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used for binding")]
        public string Status => "Unresolved";

        /// <summary>
        /// Gets the alert's monitor service.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used for binding")]
        public string MonitorService => "Azure monitor";
    }
}