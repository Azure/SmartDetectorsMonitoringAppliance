//-----------------------------------------------------------------------
// <copyright file="ResultItemPresentationSection.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    /// <summary>
    /// An enumeration of possible section where Smart Signal result item properties can be presented.
    /// </summary>
    public enum ResultItemPresentationSection
    {
        /// <summary>
        /// Indicates a property belonging to the properties section.
        /// </summary>
        Property,

        /// <summary>
        /// Indicates a property belonging to the analysis section.
        /// </summary>
        Analysis,

        /// <summary>
        /// Indicates a property belonging to the charts section.
        /// </summary>
        Chart,

        /// <summary>
        /// Indicates a property belonging to the additional queries section.
        /// </summary>
        AdditionalQuery
    }
}
