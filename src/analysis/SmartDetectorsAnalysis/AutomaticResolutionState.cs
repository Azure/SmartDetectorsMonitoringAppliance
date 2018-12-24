//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System.Collections.Generic;

    /// <summary>
    /// A class encapsulating the automatic resolution state, used to extract all relevant
    /// alert information for supplying the detector on automatic resolution checks.
    /// </summary>
    public class AutomaticResolutionState
    {
        /// <summary>
        /// Gets or sets the alert predicates this state relates to.
        /// </summary>
        public IReadOnlyDictionary<string, object> AlertPredicates { get; set; }
    }
}
