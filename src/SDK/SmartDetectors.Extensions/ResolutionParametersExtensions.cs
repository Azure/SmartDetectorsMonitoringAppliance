//-----------------------------------------------------------------------
// <copyright file="ResolutionParametersExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using ContractsResolutionParameters = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ResolutionParameters;

    /// <summary>
    /// Extension methods for the <see cref="AlertResolutionParameters"/> class.
    /// </summary>
    public static class ResolutionParametersExtensions
    {
        /// <summary>
        /// Converts the resolution parameters from the Smart Detectors SDK class to the Runtime Environment contracts class.
        /// </summary>
        /// <param name="parameters">The Smart Detectors SDK resolution parameters.</param>
        /// <returns>The Runtime Environment contracts resolution parameters.</returns>
        public static ContractsResolutionParameters CreateContractsResolutionParameters(this AlertResolutionParameters parameters)
        {
            return new ContractsResolutionParameters
            {
                CheckForResolutionAfter = parameters.CheckForResolutionAfter
            };
        }
    }
}
