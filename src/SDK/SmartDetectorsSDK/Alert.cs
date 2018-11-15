//-----------------------------------------------------------------------
// <copyright file="Alert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// The base class for representing a specific Alert.
    /// Each Alert instance contains both the detected issue's data and representation properties.
    /// </summary>
    public abstract class Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        protected Alert(string title, ResourceIdentifier resourceIdentifier)
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
        /// Gets the title of this Alert.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the resource identifier that this Alert applies to.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier { get; }

        /// <summary>
        /// Gets or sets optional automatic resolution parameters for this Alert.
        /// </summary>
        public AutomaticResolutionParameters AutomaticResolutionParameters { get; set; }
    }
}
