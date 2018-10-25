//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTableAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    /// <summary>
    /// An attribute defining the presentation of a columnar table property in an <see cref="Alert"/>.
    /// </summary>
    public class AlertPresentationTableAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationTableAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        public AlertPresentationTableAttribute(string displayName)
            : base(displayName)
        {
            this.ShowHeaders = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether headers should be displayed for this table property
        /// </summary>
        public bool ShowHeaders { get; set; }
    }
}
