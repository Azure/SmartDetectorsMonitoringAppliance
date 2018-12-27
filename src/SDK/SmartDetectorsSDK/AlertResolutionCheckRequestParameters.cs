//-----------------------------------------------------------------------
// <copyright file="AlertResolutionCheckRequestParameters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Encapsulates the alert resolution check request parameters as received from the Azure Monitoring back-end.
    /// </summary>
    public class AlertResolutionCheckRequestParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertResolutionCheckRequestParameters"/> class.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier for which the Alert was fired.</param>
        /// <param name="alertFireTime">The fire time of the alert to check for resolution.</param>
        /// <param name="alertPredicates">A dictionary containing the predicates of the alert to check for resolution.</param>
        public AlertResolutionCheckRequestParameters(
            ResourceIdentifier resourceIdentifier,
            DateTime alertFireTime,
            IReadOnlyDictionary<string, object> alertPredicates)
        {
            if (alertPredicates == null)
            {
                throw new ArgumentNullException(nameof(alertPredicates));
            }

            this.ResourceIdentifier = resourceIdentifier;
            this.AlertFireTime = alertFireTime;
            this.AlertPredicates = alertPredicates;
        }

        /// <summary>
        /// Gets the resource identifier for which the Alert was fired.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier { get; }

        /// <summary>
        /// Gets the fire time of the alert to check for resolution.
        /// </summary>
        public DateTime AlertFireTime { get; }

        /// <summary>
        /// Gets a dictionary containing the predicates of the alert to check for resolution.
        /// </summary>
        public IReadOnlyDictionary<string, object> AlertPredicates { get; }
    }
}
