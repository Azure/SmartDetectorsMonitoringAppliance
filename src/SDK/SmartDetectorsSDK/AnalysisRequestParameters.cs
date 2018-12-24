//-----------------------------------------------------------------------
// <copyright file="AnalysisRequestParameters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Encapsulates all the analysis request parameters as received from the Azure Monitoring back-end.
    /// </summary>
    public class AnalysisRequestParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisRequestParameters"/> class.
        /// </summary>
        /// <param name="targetResources">The list of resource identifiers to analyze.</param>
        /// <param name="analysisCadence">The analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.</param>
        /// <param name="alertRuleResourceId">The Alert Rule resource ID.</param>
        /// <param name="detectorParameters">The detector parameters as specified in the Alert Rule.</param>
        public AnalysisRequestParameters(
            List<ResourceIdentifier> targetResources,
            TimeSpan analysisCadence,
            string alertRuleResourceId,
            IDictionary<string, object> detectorParameters)
        {
            // Parameter validations
            if (targetResources == null)
            {
                throw new ArgumentNullException(nameof(targetResources));
            }
            else if (!targetResources.Any())
            {
                throw new ArgumentException("An Analysis Request must have at least one target resource", nameof(targetResources));
            }

            if (analysisCadence <= TimeSpan.Zero)
            {
                throw new ArgumentException("The analysis cadence must represent a positive time span", nameof(analysisCadence));
            }

            this.TargetResources = targetResources;
            this.AnalysisCadence = analysisCadence;
            this.AlertRuleResourceId = alertRuleResourceId;
            this.DetectorParameters = detectorParameters ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the list of resource identifiers to analyze.
        /// <para>
        /// The scope of analysis depends on the resource's type, so that for resources with types that represent
        /// a container resource (such as <see cref="ResourceType.Subscription"/> or <see cref="ResourceType.ResourceGroup"/>),
        /// the Smart Detector is expected to analyze all relevant resources contained in that container.</para>
        /// </summary>
        public List<ResourceIdentifier> TargetResources { get; }

        /// <summary>
        /// Gets the analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.
        /// </summary>
        public TimeSpan AnalysisCadence { get; }

        /// <summary>
        /// Gets the alert rule resource ID.
        /// </summary>
        public string AlertRuleResourceId { get; }

        /// <summary>
        /// Gets the detector parameters as specified in the Alert Rule.
        /// </summary>
        public IDictionary<string, object> DetectorParameters { get; }
    }
}
