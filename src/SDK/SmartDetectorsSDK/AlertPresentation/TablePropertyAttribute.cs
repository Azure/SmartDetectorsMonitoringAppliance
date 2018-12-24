//-----------------------------------------------------------------------
// <copyright file="TablePropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of a columnar table property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public abstract class TablePropertyAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TablePropertyAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        protected TablePropertyAttribute(string displayName)
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
