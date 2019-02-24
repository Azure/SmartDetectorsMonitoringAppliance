//-----------------------------------------------------------------------
// <copyright file="ChartPropertyToChartPropertyControlViewModelConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from a <see cref="ChartAlertProperty"/> value to <see cref="ChartPropertyControlViewModel"/>.
    /// </summary>
    public class ChartPropertyToChartPropertyControlViewModelConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This 'if' clause was added to workaround a known issue in WPF -
            // DataContext of a 'ListViewItem' is being assigned with '{DisconnectedItem}' object and then a null value whenever its container is closed.
            if (value == null || value.ToString() == "{DisconnectedItem}")
            {
                return value;
            }

            if (value.GetType() == typeof(ChartAlertProperty))
            {
                var chartlertProperty = value as ChartAlertProperty;
                return new ChartPropertyControlViewModel(chartlertProperty);
            }
            else if (value.GetType() == typeof(ChartAlertPropertiesContainer))
            {
                var chartAlertPropertiesContainer = value as ChartAlertPropertiesContainer;
                return new ChartPropertyControlViewModel(chartAlertPropertiesContainer);
            }

            throw new ArgumentException($"The value parameter must be of type {nameof(ChartAlertProperty)} or {nameof(ChartPropertyControlViewModel)}, but it is from type {value.GetType().Name}.", nameof(value));
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
