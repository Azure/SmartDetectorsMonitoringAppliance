//-----------------------------------------------------------------------
// <copyright file="AlertPresentationPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a specific property in an Alert.
    /// The attribute determines which section the property will be presented in, the display title for the
    /// property and an optional info balloon.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AlertPresentationPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationPropertyAttribute"/> class.
        /// </summary>
        /// <param name="section">The section in which the property will be presented.</param>
        /// <param name="title">The title to use when presenting the property's value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="title"/> is null or contains only white-spaces.</exception>
        public AlertPresentationPropertyAttribute(AlertPresentationSection section, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "A property cannot be presented without a title");
            }

            this.Section = section;
            this.Title = title;
        }

        /// <summary>
        /// Gets the section in which the property will be presented.
        /// </summary>
        public AlertPresentationSection Section { get; }

        /// <summary>
        /// Gets the title to use when presenting the property's value.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets or sets an (optional) info balloon to show when hovering over the property's presentation.
        /// </summary>
        public string InfoBalloon { get; set; }

        /// <summary>
        /// Gets or sets the order (optional) in which the property will be presented.
        /// </summary>
        public byte Order { get; set; } = byte.MaxValue;
    }
}
