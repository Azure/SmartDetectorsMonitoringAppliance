//-----------------------------------------------------------------------
// <copyright file="AlertPresentationTableColumnAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a column in a table defined by <see cref="AlertPresentationTableAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AlertPresentationTableColumnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertPresentationTableColumnAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The column's display name</param>
        public AlertPresentationTableColumnAttribute(string displayName)
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
