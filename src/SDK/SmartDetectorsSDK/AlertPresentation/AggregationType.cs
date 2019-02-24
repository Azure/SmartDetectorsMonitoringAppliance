//-----------------------------------------------------------------------
// <copyright file="AggregationType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// Specified the possible aggregation to use when displaying a metric in a chart
    /// </summary>
    public enum AggregationType
    {
        /// <summary>
        /// An average aggregation.
        /// </summary>
        Average,

        /// <summary>
        /// A sum aggregation.
        /// </summary>
        Sum,

        /// <summary>
        /// A count aggregation.
        /// </summary>
        Count
    }
}
