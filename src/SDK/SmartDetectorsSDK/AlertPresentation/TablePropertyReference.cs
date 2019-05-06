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
        public TablePropertyReference(string referencePath)
            : base(referencePath)
        {
        }
    }
}
