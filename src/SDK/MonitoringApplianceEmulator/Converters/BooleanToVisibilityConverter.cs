//-----------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from a <see cref="bool"/> value to <see cref="Visibility"/>.
    /// The default result will be <see cref="Visibility.Visible"/> for a value of <c>true</c>, and <see cref="Visibility.Collapsed"/> otherwise.
    /// The result can be tuned by supplying the converter parameter, which should contain a string of two values of <see cref="Visibility"/> literals
    /// and separated by comma (i.e. "Hidden,Visible") - the first of which represents the "true" result and the second the "zero" result.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
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
            if (!(value is bool))
            {
                return Visibility.Collapsed;
            }

            Visibility trueValue = Visibility.Visible;
            Visibility falseValue = Visibility.Collapsed;
            string stringParameter = parameter as string;
            if (!string.IsNullOrEmpty(stringParameter))
            {
                Visibility[] parameterParts = stringParameter.Split(',').Select(part => (Visibility)Enum.Parse(typeof(Visibility), part)).ToArray();
                trueValue = parameterParts[0];
                falseValue = parameterParts[1];
            }

            return (bool)value ? trueValue : falseValue;
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
