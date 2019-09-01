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
        /// <param name="isOptional">
        /// The flag indicating whether the property is optional.
        /// If the property is optional and the reference path is incorrect or leads to a null value - then the property will not be diplayed
        /// </param>
        /// <param name="isPropertySerialized">
        /// The flag indicating whether the property referenced by <paramref name="referencePath"/> is serialized
        /// </param>
        public PropertyReference(string referencePath, bool isOptional = false, bool isPropertySerialized = false)
        {
            if (string.IsNullOrEmpty(referencePath))
            {
                throw new ArgumentNullException(nameof(referencePath));
            }

            this.ReferencePath = referencePath;
            this.IsOptional = isOptional;
            this.IsPropertySerialized = isPropertySerialized;
        }

        /// <summary>
        /// Gets the path to the referenced property. The path can be a dot delimited path if it references
        /// a nested property.
        /// </summary>
        public string ReferencePath { get; }

        /// <summary>
        /// Gets a value indicating whether the property is optional.
        /// If the property is optional and the reference path is incorrect or leads to a null value - then the property will not be diplayed
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Gets a value indicating whether the property referenced by <see cref="ReferencePath"/> is serialized
        /// </summary>
        public bool IsPropertySerialized { get; }
    }
}
