//-----------------------------------------------------------------------
// <copyright file="AlertDetailsControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="AlertDetailsControl"/> control.
    /// </summary>
    public class AlertDetailsControlViewModel : ObservableObject
    {
        private readonly List<AlertPropertyType> supportedPropertiesTypes = new List<AlertPropertyType>()
        {
            AlertPropertyType.Text,
            AlertPropertyType.LongText,
            AlertPropertyType.KeyValue,
            AlertPropertyType.Table,
            AlertPropertyType.Chart
        };

        private EmulationAlert alert;

        private ObservableCollection<AzureResourceProperty> essentialsSectionProperties;

        private ObservableCollection<DisplayableAlertProperty> displayableProperties;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertDetailsControlViewModel"/> class for design time only.
        /// </summary>
        public AlertDetailsControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertDetailsControlViewModel" /> class.
        /// </summary>
        /// <param name="alert">The alert.</param>
        /// <param name="alertDetailsControlClosed">Handler for closing the details control.</param>
        [InjectionConstructor]
        public AlertDetailsControlViewModel(
            EmulationAlert alert,
            AlertDetailsControlClosedEventHandler alertDetailsControlClosed)
        {
            this.Alert = alert;

            this.EssentialsSectionProperties = new ObservableCollection<AzureResourceProperty>(new List<AzureResourceProperty>()
                {
                    new AzureResourceProperty("Subscription id", this.Alert.ResourceIdentifier.SubscriptionId),
                    new AzureResourceProperty("Resource group", this.Alert.ResourceIdentifier.ResourceGroupName),
                    new AzureResourceProperty("Resource type", this.Alert.ResourceIdentifier.ResourceType.ToString()),
                    new AzureResourceProperty("Resource name", this.Alert.ResourceIdentifier.ResourceName)
                });

            List<DisplayableAlertProperty> displayableAlertProperties = this.Alert.ContractsAlert.AlertProperties.OfType<DisplayableAlertProperty>()
                .Where(prop => this.supportedPropertiesTypes.Contains(prop.Type))
                .OrderBy(prop => prop.Order)
                .ThenBy(prop => prop.PropertyName)
                .ToList();

            this.DisplayableProperties = new ObservableCollection<DisplayableAlertProperty>(displayableAlertProperties);

            this.CloseControlCommand = new CommandHandler(() =>
            {
                alertDetailsControlClosed.Invoke();
            });
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets the alert.
        /// </summary>
        public Models.EmulationAlert Alert
        {
            get
            {
                return this.alert;
            }

            private set
            {
                this.alert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the essentials section's properties.
        /// </summary>
        public ObservableCollection<AzureResourceProperty> EssentialsSectionProperties
        {
            get
            {
                return this.essentialsSectionProperties;
            }

            private set
            {
                this.essentialsSectionProperties = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the alert properties' view mmodels.
        /// </summary>
        public ObservableCollection<DisplayableAlertProperty> DisplayableProperties
        {
            get
            {
                return this.displayableProperties;
            }

            private set
            {
                this.displayableProperties = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that runs the Smart Detector.
        /// </summary>
        public CommandHandler CloseControlCommand { get; }

        #endregion
    }
}
