//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionParameters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// Contains parameters used to handle alerts' automatic resolution
    /// </summary>
    public class ResolutionParameters
    {
        /// <summary>
        /// Gets or sets the duration after which the Alert will be checked for automatic
        /// resolution by querying the Runtime Environment.
        /// </summary>
        public TimeSpan CheckForAutomaticResolutionAfter { get; set; }
    }
}
