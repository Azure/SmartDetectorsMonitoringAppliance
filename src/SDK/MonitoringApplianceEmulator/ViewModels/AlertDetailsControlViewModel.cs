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
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Win32;
    using Unity.Attributes;
    using Unity.Interception.Utilities;

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

            // Project CAD chart displayable properties
            List<ChartAlertProperty> cadChartAlertProperties = this.Alert.ContractsAlert.AlertProperties.OfType<ChartAlertProperty>()
                .Where(chartProp => chartProp.DisplayName.EndsWith("_Value", StringComparison.InvariantCulture) || chartProp.DisplayName.EndsWith("_Low", StringComparison.InvariantCulture) || chartProp.DisplayName.EndsWith("_High", StringComparison.InvariantCulture) || chartProp.DisplayName.EndsWith("_Anomalies", StringComparison.InvariantCulture))
                .ToList();

            // Project all other displayable properties
            List<DisplayableAlertProperty> displayableAlertProperties = this.Alert.ContractsAlert.AlertProperties.OfType<DisplayableAlertProperty>()
                .Where(prop => this.supportedPropertiesTypes.Contains(prop.Type))
                .Where(prop => !prop.DisplayName.EndsWith("_Value", StringComparison.InvariantCulture) && !prop.DisplayName.EndsWith("_Low", StringComparison.InvariantCulture) && !prop.DisplayName.EndsWith("_High", StringComparison.InvariantCulture) && !prop.DisplayName.EndsWith("_Anomalies", StringComparison.InvariantCulture))
                .ToList();

            // Group all CAD chart properties by chart display name
            Dictionary<string, List<ChartAlertProperty>> chartNamesToContainers = new Dictionary<string, List<ChartAlertProperty>>();

            foreach (var chartProperty in cadChartAlertProperties)
            {
                string chartDisplayNamePrefix = chartProperty.DisplayName;
                int delimiterPosition = chartDisplayNamePrefix.LastIndexOf('_');
                if (delimiterPosition >= 0)
                {
                    chartDisplayNamePrefix = chartDisplayNamePrefix.Substring(0, delimiterPosition);
                }

                if (chartNamesToContainers.ContainsKey(chartDisplayNamePrefix))
                {
                    chartNamesToContainers[chartDisplayNamePrefix].Add(chartProperty);
                }
                else
                {
                    chartNamesToContainers.Add(chartDisplayNamePrefix, new List<ChartAlertProperty>() { chartProperty });
                }
            }

            List<ChartAlertPropertiesContainer> chartsContainers = new List<ChartAlertPropertiesContainer>();
            chartNamesToContainers.Values.ForEach(chartPropertiesList =>
            {
                // Set the chart container Order property to be as the Order of 'Value' chart property
                byte chartsContainersPropertyOrder = chartPropertiesList
                    .First(chartProp => chartProp.DisplayName.EndsWith("_Value", StringComparison.InvariantCulture))
                    .Order;

                chartsContainers.Add(new ChartAlertPropertiesContainer(chartPropertiesList, chartsContainersPropertyOrder));
            });

            displayableAlertProperties.AddRange(chartsContainers);

            displayableAlertProperties = new List<DisplayableAlertProperty>(displayableAlertProperties
                .OrderBy(prop => prop.Order)
                .ThenBy(prop => prop.PropertyName));

            this.DisplayableProperties = new ObservableCollection<DisplayableAlertProperty>(displayableAlertProperties);

            this.CloseControlCommand = new CommandHandler(() =>
            {
                alertDetailsControlClosed.Invoke();
            });

            this.SaveAsImageControlCommand = new CommandHandler(SaveControlAsImage);
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
        /// Gets the command that saves the details as image
        /// </summary>
        public CommandHandler SaveAsImageControlCommand { get; }

        #endregion

        #region Private methods

        /// <summary>
        /// Saves teh specified control to an image file
        /// </summary>
        /// <param name="obj">The control</param>
        private static void SaveControlAsImage(object obj)
        {
            if (!(obj is UIElement control))
            {
                return;
            }

            // Select image file path
            var dialog = new SaveFileDialog { Filter = "Jpeg Files (*.jpg)|*.jpg" };
            if (dialog.ShowDialog() == false)
            {
                return;
            }

            // Capture the control in a DrawingVisual object
            Rect bounds = VisualTreeHelper.GetDescendantBounds(control);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(new VisualBrush(control), null, new Rect(new Point(0, 0), bounds.Size));
            }

            // Render the visual to a bitmap
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            // Convert the bitmap to a jpg file
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (Stream s = new FileStream(dialog.FileName, FileMode.Create))
            {
                encoder.Save(s);
            }
        }

        #endregion
    }
}
