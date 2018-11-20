//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionParametersExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using AutomaticResolutionParameters = Microsoft.Azure.Monitoring.SmartDetectors.AutomaticResolutionParameters;
    using ContractsAutomaticResolutionParameters = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AutomaticResolutionParameters;

    /// <summary>
    /// Extension methods for the <see cref="AutomaticResolutionParameters"/> class.
    /// </summary>
    public static class AutomaticResolutionParametersExtensions
    {
        /// <summary>
        /// Converts the automatic resolution parameters from the Smart Detectors SDK class to the Runtime Environment contracts class.
        /// </summary>
        /// <param name="parameters">The Smart Detectors SDK automatic resolution parameters.</param>
        /// <returns>The Runtime Environment contracts automatic resolution parameters.</returns>
        public static ContractsAutomaticResolutionParameters CreateContractsAutomaticResolutionParameters(this AutomaticResolutionParameters parameters)
        {
            return new ContractsAutomaticResolutionParameters
            {
                CheckForAutomaticResolutionAfter = parameters.CheckForAutomaticResolutionAfter
            };
        }
    }
}
