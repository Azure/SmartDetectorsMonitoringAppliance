//-----------------------------------------------------------------------
// <copyright file="LongTextPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a long text property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class LongTextPropertyAttribute : AlertPresentationPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongTextPropertyAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        public LongTextPropertyAttribute(string displayName)
            : base(displayName)
        {
        }

        /// <summary>
        /// Gets or sets the format string to use when converting the property's value to a string
        /// For example, if the value is a <see cref="DateTime"/>, the format string can be "The incident happened on {0:u}"
        /// </summary>
        public string FormatString { get; set; } = string.Empty;
    }
}
