//-----------------------------------------------------------------------
// <copyright file="ThresholdType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// Enumeration of possible types of a metric chart threshold.
    /// </summary>
    public enum ThresholdType
    {
        /// <summary>
        /// Indicates a greater-than threshold.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Indicates a less-than threshold.
        /// </summary>
        LessThan
    }
}
