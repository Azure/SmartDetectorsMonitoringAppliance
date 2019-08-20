//-----------------------------------------------------------------------
// <copyright file="AlertDetailsControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Newtonsoft.Json.Linq;
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
            AlertPropertyType.Chart,
            AlertPropertyType.MetricChart
        };

        private EmulationAlert alert;

        private ObservableCollection<AzureResourceProperty> essentialsSectionProperties;

        private ObservableTask<ObservableCollection<DisplayableAlertProperty>> displayablePropertiesTask;

        #region Constructors

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
        /// <param name="armClient">Support for ARM request.</param>
        [InjectionConstructor]

        public AlertDetailsControlViewModel(
            EmulationAlert alert,
            AlertDetailsControlClosedEventHandler alertDetailsControlClosed,
            IAzureResourceManagerClient armClient)
        {
            this.Alert = alert;

            this.EssentialsSectionProperties = new ObservableCollection<AzureResourceProperty>(new List<AzureResourceProperty>()
                {
                    new AzureResourceProperty("Subscription id", this.Alert.ResourceIdentifier.SubscriptionId),
                    new AzureResourceProperty("Resource group", this.Alert.ResourceIdentifier.ResourceGroupName),
                    new AzureResourceProperty("Resource type", this.Alert.ResourceIdentifier.ResourceType.ToString()),
                    new AzureResourceProperty("Resource name", this.Alert.ResourceIdentifier.ResourceName)
                });

            this.DisplayablePropertiesTask = new ObservableTask<ObservableCollection<DisplayableAlertProperty>>(
                this.ComposeAlertProperties(armClient), null);

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
        /// Gets the alert properties' view models.
        /// </summary>
        public ObservableTask<ObservableCollection<DisplayableAlertProperty>> DisplayablePropertiesTask
        {
            get
            {
                return this.displayablePropertiesTask;
            }

            private set
            {
                this.displayablePropertiesTask = value;
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

        private async Task<List<DisplayableAlertProperty>> ComposeArmProperties(AzureResourceManagerRequestAlertProperty armProperty, IAzureResourceManagerClient armClient)
        {
            var url = armProperty.AzureResourceManagerRequestUri;
            IReadOnlyList<DisplayableAlertProperty> propertyRefs = armProperty.PropertiesToDisplay;
            List<DisplayableAlertProperty> displayableArmProperties = new List<DisplayableAlertProperty>();
            try
            {
                List<JObject> response = await armClient.ExecuteArmQueryAsync(url, CancellationToken.None);

                foreach (IReferenceAlertProperty propertyRef in propertyRefs.OfType<IReferenceAlertProperty>())
                {
                    JToken propertyVal = response[0].SelectToken(propertyRef.ReferencePath);

                    switch (propertyRef)
                    {
                        case TextReferenceAlertProperty textRef:
                            TextAlertProperty displayText = new TextAlertProperty(textRef.PropertyName, textRef.DisplayName, textRef.Order, (string)propertyVal);
                            displayableArmProperties.Add(displayText);
                            break;

                        case LongTextReferenceAlertProperty longTextRef:
                            LongTextAlertProperty displayLongText = new LongTextAlertProperty(longTextRef.PropertyName, longTextRef.DisplayName, longTextRef.Order, (string)propertyVal);
                            displayableArmProperties.Add(displayLongText);
                            break;

                        case KeyValueReferenceAlertProperty keyValueRef:
                            IDictionary<string, string> keyValueField = new Dictionary<string, string> { { keyValueRef.ReferencePath, (string)propertyVal } };
                            KeyValueAlertProperty displayKeyValue = new KeyValueAlertProperty(keyValueRef.PropertyName, keyValueRef.DisplayName, keyValueRef.Order, keyValueField);
                            displayableArmProperties.Add(displayKeyValue);
                            break;

                        case TableReferenceAlertProperty tableRef:
                            JArray tableValue;
                            if (response.Count == 1)
                            {
                                tableValue = response[0].SelectToken(tableRef.ReferencePath) as JArray;
                            }
                            else
                            {
                                tableValue = (new JArray(response)).SelectToken(tableRef.ReferencePath) as JArray;
                            }

                            List<Dictionary<string, JToken>> values = tableValue
                                .OfType<IDictionary<string, JToken>>()
                                .Select(value => value.ToDictionary(item => item.Key, item => item.Value))
                                .ToList();
                            TableAlertProperty<Dictionary<string, JToken>> displayTable = new TableAlertProperty<Dictionary<string, JToken>>(tableRef.PropertyName, tableRef.DisplayName, tableRef.Order, true, tableRef.Columns, values);
                            displayableArmProperties.Add(displayTable);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                string errorValue = $"Failed to get Arm Response, Error: {e.Message}";

                foreach (IReferenceAlertProperty propertyRef in propertyRefs.OfType<IReferenceAlertProperty>())
                {
                    switch (propertyRef)
                    {
                        case TextReferenceAlertProperty textRef:
                            TextAlertProperty displayText = new TextAlertProperty(textRef.PropertyName, textRef.DisplayName, textRef.Order, errorValue);
                            displayableArmProperties.Add(displayText);
                            break;

                        case LongTextReferenceAlertProperty longTextRef:
                            LongTextAlertProperty displayLongText = new LongTextAlertProperty(longTextRef.PropertyName, longTextRef.DisplayName, longTextRef.Order, errorValue);
                            displayableArmProperties.Add(displayLongText);
                            break;

                        case KeyValueReferenceAlertProperty keyValueRef:
                            IDictionary<string, string> keyValueField = new Dictionary<string, string> { { keyValueRef.ReferencePath, errorValue } };
                            KeyValueAlertProperty displayKeyValue = new KeyValueAlertProperty(keyValueRef.PropertyName, keyValueRef.DisplayName, keyValueRef.Order, keyValueField);
                            displayableArmProperties.Add(displayKeyValue);
                            break;

                        case TableReferenceAlertProperty tableRef:
                            displayText = new TextAlertProperty(tableRef.PropertyName, tableRef.DisplayName, tableRef.Order, errorValue);
                            displayableArmProperties.Add(displayText);
                            break;
                    }
                }
            }

            return displayableArmProperties;
        }

        private async Task<ObservableCollection<DisplayableAlertProperty>> ComposeAlertProperties(IAzureResourceManagerClient armClient)
        {
            try
            {
                IEnumerable<Task<List<DisplayableAlertProperty>>> armPropertiesTasks = this.Alert.ContractsAlert.AlertProperties.OfType<AzureResourceManagerRequestAlertProperty>()
                    .Select(armProperty => this.ComposeArmProperties(armProperty, armClient));
                List<DisplayableAlertProperty> armProperties = (await Task.WhenAll(armPropertiesTasks)).SelectMany(props => props).ToList();

                List<DisplayableAlertProperty> displayableAlertProperties = this.Alert.ContractsAlert.AlertProperties.OfType<DisplayableAlertProperty>()
                    .Where(prop => this.supportedPropertiesTypes.Contains(prop.Type))
                    .Union(armProperties)
                    .OrderBy(prop => prop.Order)
                    .ThenBy(prop => prop.PropertyName)
                    .ToList();

                return new ObservableCollection<DisplayableAlertProperty>(displayableAlertProperties);
            }
            catch (Exception e)
            {
                throw new InvalidSmartDetectorPackageException($"Failed to create alert details for display, Error: {e.Message}", e);
            }
        }
    }
}
