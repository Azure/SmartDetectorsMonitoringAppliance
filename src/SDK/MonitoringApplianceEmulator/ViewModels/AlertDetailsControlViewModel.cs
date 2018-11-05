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
    using System.Configuration;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Unity.Attributes;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// The view model class for the <see cref="AlertDetailsControl"/> control.
    /// </summary>
    public class AlertDetailsControlViewModel : ObservableObject
    {
        private readonly ISystemProcessClient systemProcessClient;

        // temporary, until all types will be supported
        private readonly List<AlertPropertyType> supportedPropertiesTypes = new List<AlertPropertyType>() { AlertPropertyType.Text, AlertPropertyType.KeyValue, AlertPropertyType.Table, AlertPropertyType.Chart };

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
        /// <param name="systemProcessClient">The system process client.</param>
        [InjectionConstructor]
        public AlertDetailsControlViewModel(
            EmulationAlert alert,
            AlertDetailsControlClosedEventHandler alertDetailsControlClosed,
            ISystemProcessClient systemProcessClient)
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

            this.systemProcessClient = systemProcessClient;
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

        /// <summary>
        /// Gets a command to open an analytics kusto query in a new browser tab.
        /// </summary>
        public CommandHandler OpenAnalyticsQueryCommand => new CommandHandler(queryParameter =>
        {
            // Get the query from the parameter
            string query = (string)queryParameter;

            // Compress it so we can add it to the query parameters
            string compressedQuery;
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    byte[] queryBtyes = Encoding.UTF8.GetBytes(query);
                    gzipStream.Write(queryBtyes, 0, queryBtyes.Length);
                }

                compressedQuery = Convert.ToBase64String(outputStream.ToArray());
            }

            // Compose the URI
            string endpoint;
            string resourceUrlParameterName;
            if (this.Alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights)
            {
                endpoint = ConfigurationManager.AppSettings["ApplicationInsightsPortalEndpoint"] ?? "analytics.applicationinsights.io";
                resourceUrlParameterName = "components";
            }
            else
            {
                endpoint = ConfigurationManager.AppSettings["LogAnalyticsPortalEndpoint"] ?? "portal.loganalytics.io";
                resourceUrlParameterName = "workspaces";
            }

            #pragma warning disable CS0612 // Type or member is obsolete;

            // Use the first resource ID from query run info for the query.
            // It might not work for Log Analytics results - since there might be few resources.
            // Anyway, this is temporary hack until there will be query visualizations in emulator.
            string alertResourceId = this.Alert.ContractsAlert.QueryRunInfo.ResourceIds[0];
            ResourceIdentifier alertResourceIdentifier = ResourceIdentifier.CreateFromResourceId(alertResourceId);

            Uri queryDeepLink =
                new Uri($"https://{endpoint}/subscriptions/{alertResourceIdentifier.SubscriptionId}/resourcegroups/{alertResourceIdentifier.ResourceGroupName}/{resourceUrlParameterName}/{alertResourceIdentifier.ResourceName}?q={compressedQuery}");

            this.systemProcessClient.StartWebBrowserProcess(queryDeepLink);

            #pragma warning restore CS0612// Type or member is obsolete;
        });

        #endregion
    }
}
