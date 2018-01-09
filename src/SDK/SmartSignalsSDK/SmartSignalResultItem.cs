//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// The base class for representing a specific item in the Smart Signal result.
    /// Each result item instance contains both the detected issue's data and representation properties.
    /// </summary>
    public abstract class SmartSignalResultItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultItem"/> class.
        /// </summary>
        /// <param name="title">The result item's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this items applies to.</param>
        protected SmartSignalResultItem(string title, ResourceIdentifier resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (resourceIdentifier == null)
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }

            this.Title = title;
            this.ResourceIdentifier = resourceIdentifier;
        }

        /// <summary>
        /// Gets the title of this result item.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the resource identifier that this items applies to.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier { get; }
    }
}
