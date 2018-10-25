//-----------------------------------------------------------------------
// <copyright file="ResourceTypeToIconConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from <see cref="ResourceType"/> values to <see cref="BitmapImage"/>.
    /// </summary>
    public class ResourceTypeToIconConverter : IValueConverter
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
            bool isSupportedResourceType = Enum.TryParse(value?.ToString(), out ResourceType resourceType);
            if (!isSupportedResourceType)
            {
                return new BitmapImage(new Uri("pack://application:,,,/Media/resource_default.png"));
            }

            switch (resourceType)
            {
                case ResourceType.Subscription:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/subscription.png"));
                case ResourceType.ResourceGroup:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/resource_group.png"));
                case ResourceType.ApplicationInsights:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/app_insights.png"));
                case ResourceType.AzureStorage:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/storage.png"));
                case ResourceType.LogAnalytics:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/log_analytics.png"));
                case ResourceType.VirtualMachine:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/virtual_machine.png"));
                case ResourceType.VirtualMachineScaleSet:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/virtual_machine_set.png"));
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/Media/resource_default.png"));
            }
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
