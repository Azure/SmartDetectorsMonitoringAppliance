//-----------------------------------------------------------------------
// <copyright file="AnalysisRequest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a single analysis request sent to the Smart Signal. This is the main parameter sent to the 
    /// <see cref="ISmartSignal.AnalyzeResourcesAsync"/> method.
    /// </summary>
    public struct AnalysisRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisRequest"/> struct.
        /// </summary>
        /// <param name="targetResources">The list of resource identifiers to analyze.</param>
        /// <param name="lastAnalysisTime">
        /// The date and time when the last successful analysis of the signal occurred. This can be null if the signal never ran.
        /// </param>
        /// <param name="analysisCadence">The analysis cadence defined in the Alert Rule which initiated the signal's analysis.</param>
        /// <param name="analysisServicesFactory">The analysis services factory to be used for querying the resources telemetry.</param>
        public AnalysisRequest(List<ResourceIdentifier> targetResources, DateTime? lastAnalysisTime, TimeSpan analysisCadence, IAnalysisServicesFactory analysisServicesFactory)
        {
            // Parameter validations
            if (targetResources == null)
            {
                throw new ArgumentNullException(nameof(targetResources));
            }
            else if (!targetResources.Any())
            {
                throw new ArgumentException("Analysis request must have at least one target resource", nameof(targetResources));
            }

            if (lastAnalysisTime.HasValue)
            {
                if (lastAnalysisTime.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Last analysis time must be specified in UTC", nameof(lastAnalysisTime));
                }
                else if (lastAnalysisTime.Value >= DateTime.UtcNow)
                {
                    throw new ArgumentException("Last analysis time cannot be in the future", nameof(lastAnalysisTime));
                }
            }

            if (analysisCadence <= TimeSpan.Zero)
            {
                throw new ArgumentException("Analysis cadence must represent a positive time span", nameof(analysisCadence));
            }

            if (analysisServicesFactory == null)
            {
                throw new ArgumentNullException(nameof(analysisServicesFactory));
            }

            this.TargetResources = targetResources;
            this.LastAnalysisTime = lastAnalysisTime;
            this.AnalysisCadence = analysisCadence;
            this.AnalysisServicesFactory = analysisServicesFactory;
        }

        /// <summary>
        /// Gets the list of resource identifiers to analyze.
        /// <para>
        /// The scope of analysis depends on the resource's type, so that for resources with types that represent 
        /// a container resource (such as <see cref="ResourceType.Subscription"/> or <see cref="ResourceType.ResourceGroup"/>),
        /// the signal is expected to analyze all relevant resources contained in that container.</para>
        /// </summary>
        public List<ResourceIdentifier> TargetResources { get; }

        /// <summary>
        /// Gets the date and time when the last successful analysis of the signal occurred.
        /// This can be <code>null</code> if the signal never ran.
        /// </summary>
        public DateTime? LastAnalysisTime { get; }

        /// <summary>
        /// Gets the analysis cadence defined in the Alert Rule which initiated the signal's analysis.
        /// </summary>
        public TimeSpan AnalysisCadence { get; }

        /// <summary>
        /// Gets the analysis services factory to be used for querying the resources telemetry.
        /// </summary>
        public IAnalysisServicesFactory AnalysisServicesFactory { get; }
    }
}
