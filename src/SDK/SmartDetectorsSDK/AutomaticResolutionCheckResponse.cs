//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionCheckResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// Encapsulates the response for an automatic resolution check request.
    /// </summary>
    public class AutomaticResolutionCheckResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionCheckResponse"/> class.
        /// </summary>
        /// <param name="shouldBeResolved">A value indicating whether the alert should be resolved.</param>
        /// <param name="resolutionParameters">
        /// The parameters controlling the continued automatic resolution flow for the alert. A <c>null</c>
        /// value indicates that the alert will never be automatically resolved.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown in cases where <paramref name="shouldBeResolved"/> is <c>true</c>, but
        /// <paramref name="resolutionParameters"/> contains a non null value.
        /// </exception>
        public AutomaticResolutionCheckResponse(bool shouldBeResolved, ResolutionParameters resolutionParameters)
        {
            if (shouldBeResolved && resolutionParameters != null)
            {
                throw new ArgumentException("Cannot resolve an alert and send automatic resolution parameters in the same response", nameof(resolutionParameters));
            }

            this.ShouldBeResolved = shouldBeResolved;
            this.ResolutionParameters = resolutionParameters;
        }

        /// <summary>
        /// Gets a value indicating whether the alert should be resolved.
        /// </summary>
        public bool ShouldBeResolved { get; }

        /// <summary>
        /// Gets the parameters controlling the continued automatic resolution flow for the alert.
        /// This value must be <c>null</c> if <see cref="ShouldBeResolved"/> is <c>true</c>.
        /// If <see cref="ShouldBeResolved"/> is <c>false</c> - a <c>null</c> value will indicates that the alert will never be automatically resolved.
        /// </summary>
        public ResolutionParameters ResolutionParameters { get; }
    }
}
