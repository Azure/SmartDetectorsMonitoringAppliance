//-----------------------------------------------------------------------
// <copyright file="ChartValuesToSeriesCollectionConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Defaults;
    using LiveCharts.Wpf;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> that is used to convert
    /// a list of chart points to a <see cref="SeriesCollection"/> instance.
    /// </summary>
    public class ChartValuesToSeriesCollectionConverter : IValueConverter
    {
        private static readonly Brush SeriesColor = Brushes.DeepSkyBlue;

        #region Implementation of IValueConverter

        /// <summary>Converts a value. </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChartValues<DateTimePoint> chartValues)
            {
                CartesianMapper<DateTimePoint> pointMapperConfig = Mappers.Xy<DateTimePoint>()
                    .X(dateTimeDataPoint => dateTimeDataPoint.DateTime.Ticks * 1.0 / TimeSpan.FromHours(1).Ticks)
                    .Y(dateTimeDataPoint => dateTimeDataPoint.Value);
                return new SeriesCollection(pointMapperConfig)
                    {
                        new LineSeries()
                        {
                            Values = chartValues,
                            Stroke = parameter as Brush ?? SeriesColor,
                            Fill = Brushes.Transparent,
                        }
                    };
            }

            return null;
        }

        /// <summary>Converts a value. </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
