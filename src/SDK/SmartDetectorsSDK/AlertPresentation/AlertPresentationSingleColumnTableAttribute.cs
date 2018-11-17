//-----------------------------------------------------------------------
// <copyright file="AlertPresentationSingleColumnTableAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of a single column table property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class AlertPresentationSingleColumnTableAttribute : AlertPresentationTableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationSingleColumnTableAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        public AlertPresentationSingleColumnTableAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
