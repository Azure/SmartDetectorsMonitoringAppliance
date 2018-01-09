//-----------------------------------------------------------------------
// <copyright file="CountToVisibilityConverter.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from an integer value to <see cref="Visibility"/>.
    /// The default result will be <see cref="Visibility.Visible"/> for any larger than zero value, and <see cref="Visibility.Collapsed"/> otherwise.
    /// The result can be tuned by supplying the converter parameter, which should contain a string of two values of <see cref="Visibility"/> literals
    /// and separated by comma (i.e. "Hidden,Visible") - the first of which represents the "greater than zero" result and the second the "zero" result.
    /// </summary>
    internal class CountToVisibilityConverter : IValueConverter
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
            if (!(value is int))
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

            return (int)value > 0 ? trueValue : falseValue;
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
