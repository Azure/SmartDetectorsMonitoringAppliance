//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTextAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of a (short) text property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class AlertPresentationTextAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationTextAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value.</param>
        public AlertPresentationTextAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
