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
        /// <param name="automaticResolutionParameters">
        /// The parameters controlling the continued automatic resolution flow for the alert. A <c>null</c>
        /// value indicates that the alert will never be automatically resolved.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown in cases where <paramref name="shouldBeResolved"/> is <c>true</c>, but
        /// <paramref name="automaticResolutionParameters"/> contains a non null value.
        /// </exception>
        public AutomaticResolutionCheckResponse(bool shouldBeResolved, AutomaticResolutionParameters automaticResolutionParameters)
        {
            if (shouldBeResolved && automaticResolutionParameters != null)
            {
                throw new ArgumentException("Cannot resolve an alert and send automatic resolution parameters in the same response", nameof(automaticResolutionParameters));
            }

            this.ShouldBeResolved = shouldBeResolved;
            this.AutomaticResolutionParameters = automaticResolutionParameters;
        }

        /// <summary>
        /// Gets a value indicating whether the alert should be resolved.
        /// </summary>
        public bool ShouldBeResolved { get; }

        /// <summary>
        /// Gets the parameters controlling the continued automatic resolution flow
        /// for the alert. This value is ignored if <see cref="ShouldBeResolved"/> contains <c>true</c>,
        /// and a <c>null</c> value indicates that the alert will never be automatically resolved.
        /// </summary>
        public AutomaticResolutionParameters AutomaticResolutionParameters { get; }
    }
}
