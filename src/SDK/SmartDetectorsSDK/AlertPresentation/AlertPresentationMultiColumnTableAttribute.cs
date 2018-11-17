//-----------------------------------------------------------------------
// <copyright file="AlertPresentationMultiColumnTableAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of a columnar table property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class AlertPresentationMultiColumnTableAttribute : AlertPresentationTableAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationMultiColumnTableAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        public AlertPresentationMultiColumnTableAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
