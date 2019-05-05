//-----------------------------------------------------------------------
// <copyright file="PropertyReference.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;

    /// <summary>
    /// This class is used for adding displayable Alert properties which take their data by referencing
    /// other properties. Main usage for this class is for adding properties to <see cref="AzureResourceManagerRequest"/>,
    /// which display the results of an ARM query.
    /// </summary>
    public class PropertyReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReference"/> class.
        /// </summary>
        /// <param name="referencePath">
        /// The path to the referenced property. The path can be a dot delimited path if it references
        /// a nested property.
        /// </param>
        public PropertyReference(string referencePath)
        {
            if (string.IsNullOrEmpty(referencePath))
            {
                throw new ArgumentNullException(nameof(referencePath));
            }

            this.ReferencePath = referencePath;
        }

        /// <summary>
        /// Gets the path to the referenced property. The path can be a dot delimited path if it references
        /// a nested property.
        /// </summary>
        public string ReferencePath { get; }
    }
}
