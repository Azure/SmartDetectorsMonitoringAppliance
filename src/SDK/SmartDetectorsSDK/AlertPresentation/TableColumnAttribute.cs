//-----------------------------------------------------------------------
// <copyright file="TableColumnAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a column in a table defined by <see cref="TablePropertyAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The column's display name</param>
        /// /// <param name="propertyName">The column's property name</param>
        public TableColumnAttribute(string displayName, string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentNullException(nameof(displayName), "A column cannot be presented without a display name");
            }

            this.DisplayName = displayName;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the display name of the column
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the property name of the column
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets or sets the order of the column in the table
        /// </summary>
        public byte Order { get; set; } = byte.MaxValue;

        /// <summary>
        /// Gets or sets the format string to use when converting the property's value to a string
        /// For example, if the value is a <see cref="DateTime"/>, the format string can be "The incident happened on {0:u}"
        /// </summary>
        public string FormatString { get; set; } = string.Empty;
    }
}
