//-----------------------------------------------------------------------
// <copyright file="IStateRepositoryFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    /// <summary>
    /// Represents a factory for creating state repository for a Smart Detector.
    /// </summary>
    public interface IStateRepositoryFactory
    {
        /// <summary>
        /// Creates a state repository for a Smart Detector with ID <paramref name="smartDetectorId"/>.
        /// </summary>
        /// <param name="smartDetectorId">The ID of the Smart Detector to create the state repository for.</param>
        /// <param name="alertRuleResourceId">The resource ID of the Alert Rule to create the state repository for.</param>
        /// <returns>A state repository associated with the requested Smart Detector.</returns>
        IStateRepository Create(string smartDetectorId, string alertRuleResourceId);
    }
}
