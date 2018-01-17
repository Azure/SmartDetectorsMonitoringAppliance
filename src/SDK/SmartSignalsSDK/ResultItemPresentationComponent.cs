//-----------------------------------------------------------------------
// <copyright file="ResultItemPresentationComponent.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// An enumeration of possible components where Smart Signal result item properties can be presented.
    /// </summary>
    [Flags]
    public enum ResultItemPresentationComponent
    {
        /// <summary>
        /// Indicates a property belonging to the details component.
        /// </summary>
        Details = 1,

        /// <summary>
        /// Indicates a property belonging to the summary component.
        /// </summary>
        Summary = 2
    }
}