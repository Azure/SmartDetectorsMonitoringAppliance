//-----------------------------------------------------------------------
// <copyright file="BooleanAndConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of <see cref="IMultiValueConverter"/> for converting <see cref="bool"/> values using AND operator.
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="values">The values produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(value => value is bool && (bool)value);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetTypes">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
