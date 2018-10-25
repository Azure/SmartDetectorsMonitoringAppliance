//-----------------------------------------------------------------------
// <copyright file="EmptyListToBooleanConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from an <see cref="int"/> value to <see cref="bool"/>.
    /// Returns true if the given value is larger than zero, otherwise false.
    /// </summary>
    public class EmptyListToBooleanConverter : IValueConverter
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
            var enumerable = value as IEnumerable;

            return enumerable != null && IsNotEmpty(enumerable);
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

        /// <summary>
        /// Checks whether an <see cref="IEnumerable"/> collection is not empty.
        /// </summary>
        /// <param name="enumerable">An enumerable collection</param>
        /// <returns>True if not empty, otherwise false</returns>
        private static bool IsNotEmpty(IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Any();
        }

        #endregion
    }
}
