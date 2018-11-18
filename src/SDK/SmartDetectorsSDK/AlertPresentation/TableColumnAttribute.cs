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
        public TableColumnAttribute(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentNullException(nameof(displayName), "A column cannot be presented without a display name");
            }

            this.DisplayName = displayName;
        }

        /// <summary>
        /// Gets the display name of the column
        /// </summary>
        public string DisplayName { get; }
    }
}
