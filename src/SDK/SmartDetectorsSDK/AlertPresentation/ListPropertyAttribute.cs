//-----------------------------------------------------------------------
// <copyright file="ListPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute that is set on a list of objects, each object defining its own presentation properties.
    /// All the properties from all the objects will be displayed, in order, in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// </summary>
    public class ListPropertyAttribute : AlertPresentationPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListPropertyAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value.</param>
        public ListPropertyAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
