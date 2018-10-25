//-----------------------------------------------------------------------
// <copyright file="AnalysisRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;

    /// <summary>
    /// Represents a single analysis request sent to the Smart Detector. This is the main parameter sent to the
    /// <see cref="ISmartDetector.AnalyzeResourcesAsync"/> method.
    /// </summary>
    public struct AnalysisRequest : IEquatable<AnalysisRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisRequest"/> struct.
        /// </summary>
        /// <param name="targetResources">The list of resource identifiers to analyze.</param>
        /// <param name="analysisCadence">The analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.</param>
        /// <param name="alertRuleResourceId">The alert rule resource ID.</param>
        /// <param name="analysisServicesFactory">The analysis services factory to be used for querying the resources telemetry.</param>
        /// <param name="stateRepository">The persistent state repository for storing state between analysis runs</param>
        public AnalysisRequest(List<ResourceIdentifier> targetResources, TimeSpan analysisCadence, string alertRuleResourceId, IAnalysisServicesFactory analysisServicesFactory, IStateRepository stateRepository)
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

            if (analysisCadence <= TimeSpan.Zero)
            {
                throw new ArgumentException("Analysis cadence must represent a positive time span", nameof(analysisCadence));
            }

            if (analysisServicesFactory == null)
            {
                throw new ArgumentNullException(nameof(analysisServicesFactory));
            }

            this.TargetResources = targetResources;
            this.AnalysisCadence = analysisCadence;
            this.AlertRuleResourceId = alertRuleResourceId;
            this.AnalysisServicesFactory = analysisServicesFactory;
            this.StateRepository = stateRepository;
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
        /// Gets the data end time to query.
        /// </summary>
        [Obsolete("DataEndTime is moved to the responsibility of the detector")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Obsolete member, about to be deprecated")]
        public DateTime DataEndTime => DateTime.UtcNow;

        /// <summary>
        /// Gets the analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.
        /// </summary>
        public TimeSpan AnalysisCadence { get; }

        /// <summary>
        /// Gets the alert rule resource ID.
        /// </summary>
        public string AlertRuleResourceId { get; }

        /// <summary>
        /// Gets the analysis services factory to be used for querying the resources telemetry.
        /// </summary>
        public IAnalysisServicesFactory AnalysisServicesFactory { get; }

        /// <summary>
        /// Gets the persistent state repository for storing state between analysis runs.
        /// </summary>
        public IStateRepository StateRepository { get; }

        #region Overrides of ValueType

        /// <summary>
        /// Determines whether two specified source identifiers have the same value.
        /// </summary>
        /// <param name="left">The first resource identifier to compare.</param>
        /// <param name="right">The second resource identifier to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> represent the same resource identifier; otherwise, false.</returns>
        public static bool operator ==(AnalysisRequest left, AnalysisRequest right)
        {
            return
                left.TargetResources.SequenceEqual(right.TargetResources) &&
                left.AnalysisCadence.Equals(right.AnalysisCadence) &&
                left.AlertRuleResourceId == right.AlertRuleResourceId;
        }

        /// <summary>
        /// Determines whether two specified source identifiers have different values.
        /// </summary>
        /// <param name="left">The first resource identifier to compare.</param>
        /// <param name="right">The second resource identifier to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> do not represent the same resource identifier; otherwise, false.</returns>
        public static bool operator !=(AnalysisRequest left, AnalysisRequest right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(AnalysisRequest other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object other)
        {
            return other is AnalysisRequest request && this == request;
        }

        /// <summary>
        /// Returns the hash code for this resource identifier.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            // Disable overflow - just in case
            unchecked
            {
                int hash = 27;
                hash = (31 * hash) + this.TargetResources.GetHashCode();
                hash = (31 * hash) + this.AnalysisCadence.GetHashCode();
                hash = (31 * hash) + (this.AlertRuleResourceId?.ToUpperInvariant().GetHashCode() ?? 0);
                return hash;
            }
        }

        #endregion
    }
}
