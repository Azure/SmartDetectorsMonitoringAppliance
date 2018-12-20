//-----------------------------------------------------------------------
// <copyright file="AlertResolutionParameters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// Contains parameters used to handle alerts' resolution
    /// </summary>
    public class AlertResolutionParameters
    {
        /// <summary>
        /// Gets or sets the duration after which the Alert will be checked for
        /// resolution by querying the Runtime Environment.
        /// </summary>
        public TimeSpan CheckForResolutionAfter { get; set; }
    }
}
