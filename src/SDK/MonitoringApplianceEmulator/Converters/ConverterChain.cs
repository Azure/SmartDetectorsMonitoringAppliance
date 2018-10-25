//-----------------------------------------------------------------------
// <copyright file="ConverterChain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// A class implementing <see cref="IValueConverter"/> by chaining several converters in a row.
    /// </summary>
    [ContentProperty("Converters")]
    [ContentWrapper(typeof(Collection<IValueConverter>))]
    public class ConverterChain : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterChain"/> class.
        /// </summary>
        public ConverterChain()
        {
            this.Converters = new Collection<IValueConverter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterChain"/> class for unit testing.
        /// </summary>
        /// <param name="converters">The converters to chain</param>
        public ConverterChain(Collection<IValueConverter> converters)
        {
            this.Converters = converters;
        }

        /// <summary>
        /// Gets the list of converters to execute.
        /// </summary>
        public Collection<IValueConverter> Converters { get; }

        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Converters
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Converters
                .Reverse()
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        #endregion
    }
}
