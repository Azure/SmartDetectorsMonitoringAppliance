//-----------------------------------------------------------------------
// <copyright file="KeyValuePropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// An attribute defining the presentation of a key-value table property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class KeyValuePropertyAttribute : AlertPresentationPropertyV2Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePropertyAttribute"/> class, for
        /// displaying a key-value table with no headers.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        public KeyValuePropertyAttribute(string displayName)
            : base(displayName)
        {
            this.ShowHeaders = false;
            this.KeyHeaderName = string.Empty;
            this.ValueHeaderName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePropertyAttribute"/> class, for
        /// displaying a key-value table with headers.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value</param>
        /// <param name="keyHeaderName">The header name of the values in the table</param>
        /// <param name="valueHeaderName">The header name of the keys in the table</param>
        public KeyValuePropertyAttribute(string displayName, string keyHeaderName, string valueHeaderName)
            : base(displayName)
        {
            if (string.IsNullOrWhiteSpace(keyHeaderName))
            {
                throw new ArgumentNullException(nameof(keyHeaderName), "A key-value property with headers must have a key header name");
            }

            if (string.IsNullOrWhiteSpace(valueHeaderName))
            {
                throw new ArgumentNullException(nameof(valueHeaderName), "A key-value property with headers must have a value header name");
            }

            this.ShowHeaders = true;
            this.KeyHeaderName = keyHeaderName;
            this.ValueHeaderName = valueHeaderName;
        }

        /// <summary>
        /// Gets a value indicating whether headers should be displayed for this key-value property
        /// </summary>
        public bool ShowHeaders { get; }

        /// <summary>
        /// Gets the header name of the keys in the table. Only applicable when <see cref="ShowHeaders"/> contains <c>true</c>
        /// </summary>
        public string KeyHeaderName { get; }

        /// <summary>
        /// Gets the header name of the values in the table. Only applicable when <see cref="ShowHeaders"/> contains <c>true</c>
        /// </summary>
        public string ValueHeaderName { get; }
    }
}
