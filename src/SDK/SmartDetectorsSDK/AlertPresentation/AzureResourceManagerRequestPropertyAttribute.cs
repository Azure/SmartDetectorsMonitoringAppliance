//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerRequestPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of an ARM Request property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// This property attribute must be applied only to properties inheriting from <see cref="AzureResourceManagerRequest"/>.
    /// </summary>
    public class AzureResourceManagerRequestPropertyAttribute : AlertPresentationPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerRequestPropertyAttribute"/> class.
        /// </summary>
        public AzureResourceManagerRequestPropertyAttribute()
            : base("N/A")
        {
        }
    }
}
