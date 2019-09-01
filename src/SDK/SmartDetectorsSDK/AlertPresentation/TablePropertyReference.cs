//-----------------------------------------------------------------------
// <copyright file="TablePropertyReference.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// A property reference class used for displaying property references in a table (to be
    /// used with <see cref="TablePropertyAttribute"/>).
    /// </summary>
    /// <typeparam name="TTableRow">The type of the table's rows.</typeparam>
    public class TablePropertyReference<TTableRow> : PropertyReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TablePropertyReference{TTableRow}"/> class.
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
        public TablePropertyReference(string referencePath, bool isOptional = false, bool isPropertySerialized = false)
            : base(referencePath, isOptional, isPropertySerialized)
        {
        }
    }
}
