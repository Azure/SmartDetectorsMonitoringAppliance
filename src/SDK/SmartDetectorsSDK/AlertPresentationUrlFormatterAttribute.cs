//-----------------------------------------------------------------------
// <copyright file="AlertPresentationUrlFormatterAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// An attribute for allowing easy formatting of <see cref="Uri"/> properties to
    /// Alert presentation properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AlertPresentationUrlFormatterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationUrlFormatterAttribute"/> class.
        /// </summary>
        /// <param name="linkText">The link display text</param>
        public AlertPresentationUrlFormatterAttribute(string linkText)
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
