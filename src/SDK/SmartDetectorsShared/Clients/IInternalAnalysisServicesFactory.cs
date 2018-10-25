//-----------------------------------------------------------------------
// <copyright file="IInternalAnalysisServicesFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    /// <summary>
    /// An interface that extends the <see cref="IAnalysisServicesFactory"/> with methods to be used internally by the monitoring appliance
    /// </summary>
    public interface IInternalAnalysisServicesFactory : IAnalysisServicesFactory
    {
        /// <summary>
        /// Gets a value indicating whether a log analysis client was used
        /// </summary>
        bool UsedLogAnalysisClient { get; }

        /// <summary>
        /// Gets a value indicating whether a log metric client was used
        /// </summary>
        bool UsedMetricClient { get; }
    }
}
