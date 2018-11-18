//-----------------------------------------------------------------------
// <copyright file="TablePropertyToTablePropertyControlViewModelConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from a <see cref="TableAlertProperty{T}"/> value to <see cref="TablePropertyControlViewModel{T}"/>.
    /// </summary>
    public class TablePropertyToTablePropertyControlViewModelConverter : IValueConverter
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
            // This 'if' clause was added to workaround a known issue in WPF - DataContext of a 'ListView' item is being assigned with '{DisconnectedItem}' object and then a null value whenever its container is closed.
            if (value == null || value.ToString() == "{DisconnectedItem}")
            {
                return value;
            }

            Type valueType = value.GetType();
            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(TableAlertProperty<>))
            {
                throw new ArgumentException($"The value parameter must be of type {typeof(TableAlertProperty<>)}, but it is from type {value.GetType()}.", nameof(value));
            }

            Type viewModelType = typeof(TablePropertyControlViewModel<>).MakeGenericType(valueType.GetGenericArguments());
            return Activator.CreateInstance(viewModelType, value);
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
