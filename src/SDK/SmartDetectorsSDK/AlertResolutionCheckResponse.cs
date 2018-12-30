//-----------------------------------------------------------------------
// <copyright file="AlertResolutionCheckResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// Encapsulates the response for an alert resolution check request.
    /// </summary>
    public class AlertResolutionCheckResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertResolutionCheckResponse"/> class.
        /// </summary>
        /// <param name="shouldBeResolved">A value indicating whether the alert should be resolved.</param>
        /// <param name="alertResolutionParameters">
        /// The parameters controlling the continued resolution flow for the alert. A <c>null</c>
        /// value indicates that the alert will never be resolved.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown in cases where <paramref name="shouldBeResolved"/> is <c>true</c>, but
        /// <paramref name="alertResolutionParameters"/> contains a non null value.
        /// </exception>
        public AlertResolutionCheckResponse(bool shouldBeResolved, AlertResolutionParameters alertResolutionParameters)
        {
            if (shouldBeResolved && alertResolutionParameters != null)
            {
                throw new ArgumentException("Cannot resolve an alert and send resolution parameters in the same response", nameof(alertResolutionParameters));
            }

            this.ShouldBeResolved = shouldBeResolved;
            this.AlertResolutionParameters = alertResolutionParameters;
        }

        /// <summary>
        /// Gets a value indicating whether the alert should be resolved.
        /// </summary>
        public bool ShouldBeResolved { get; }

        /// <summary>
        /// Gets the parameters controlling the continued resolution flow for the alert.
        /// This value must be <c>null</c> if <see cref="ShouldBeResolved"/> is <c>true</c>.
        /// If <see cref="ShouldBeResolved"/> is <c>false</c> - a <c>null</c> value will indicates that the alert will never be resolved.
        /// </summary>
        public AlertResolutionParameters AlertResolutionParameters { get; }
    }
}
