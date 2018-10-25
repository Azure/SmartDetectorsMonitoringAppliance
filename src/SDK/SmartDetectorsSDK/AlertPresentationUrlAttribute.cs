//-----------------------------------------------------------------------
// <copyright file="AlertPresentationUrlAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a URL property in an <see cref="Alert"/>.
    /// </summary>
    public class AlertPresentationUrlAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationUrlAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value.</param>
        /// <param name="linkText">The link display text</param>
        public AlertPresentationUrlAttribute(string displayName, string linkText)
            : base(displayName)
        {
            // Validate that the link text is not null of whitespaces
            if (string.IsNullOrWhiteSpace(linkText))
            {
                throw new ArgumentNullException(nameof(linkText));
            }

            this.LinkText = linkText;
        }

        /// <summary>
        /// Gets the link display Text
        /// </summary>
        public string LinkText { get; }
    }
}
