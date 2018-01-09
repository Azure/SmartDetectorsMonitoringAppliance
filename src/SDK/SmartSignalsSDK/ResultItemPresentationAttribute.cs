//-----------------------------------------------------------------------
// <copyright file="ResultItemPresentationAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a specific property in a Smart Signal result item.
    /// The attribute determines which section the property will be presented in, the display title for the
    /// property and an optional info balloon.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ResultItemPresentationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultItemPresentationAttribute"/> class.
        /// </summary>
        /// <param name="section">The section in which the property will be presented.</param>
        /// <param name="title">The title to use when presenting the property's value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="title"/> is null or contains only white-spaces.</exception>
        public ResultItemPresentationAttribute(ResultItemPresentationSection section, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "A property cannot be presented without a title");
            }

            this.Section = section;
            this.Title = title;
            this.Component = ResultItemPresentationComponent.Details;
        }

        /// <summary>
        /// Gets the section in which the property will be presented.
        /// </summary>
        public ResultItemPresentationSection Section { get; }

        /// <summary>
        /// Gets or sets a value indicating the component that this property appears in.
        /// For any Smart Signal result item, the <see cref="ResultItemPresentationComponent.Summary"/> component must
        /// have exactly one property belonging to the <see cref="ResultItemPresentationSection.Property"/> section,
        /// and at most one property belonging to the <see cref="ResultItemPresentationSection.Chart"/> section.
        /// </summary>
        public ResultItemPresentationComponent Component { get; set; }

        /// <summary>
        /// Gets the title to use when presenting the property's value.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets or sets  an (optional) info balloon to show when hovering over the property's presentation.
        /// </summary>
        public string InfoBalloon { get; set; }
    }
}
