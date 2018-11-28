//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierToResourceNameConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of <see cref="IValueConverter"/> for converting from <see cref="ResourceIdentifier"/> to its target name.
    /// </summary>
    public class ResourceIdentifierToResourceNameConverter : IValueConverter
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
            ResourceIdentifier? resourceIdentifier = value as ResourceIdentifier?;

            if (resourceIdentifier == null || resourceIdentifier.Value == null)
            {
                return string.Empty;
            }

            switch (resourceIdentifier.Value.ResourceType)
            {
                case ResourceType.Subscription:
                    return resourceIdentifier.Value.SubscriptionId;
                case ResourceType.ResourceGroup:
                    return resourceIdentifier.Value.ResourceGroupName;
                default:
                    return resourceIdentifier.Value.ResourceName;
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
