//-----------------------------------------------------------------------
// <copyright file="ResolutionState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A class encapsulating the resolution state, used to extract all relevant
    /// alert information for supplying the detector on resolution checks.
    /// </summary>
    public class ResolutionState
    {
        /// <summary>
        /// Gets or sets the original time the analysis request was received from Azure Monitor back-end.
        /// </summary>
        public DateTime AnalysisRequestTime { get; set; }

        /// <summary>
        /// Gets or sets the alert predicates this state relates to.
        /// </summary>
        public IReadOnlyDictionary<string, object> AlertPredicates { get; set; }
    }
}
