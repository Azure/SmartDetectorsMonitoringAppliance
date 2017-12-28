//-----------------------------------------------------------------------
// <copyright file="ISmartSignalResultPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using Microsoft.Azure.Monitoring.SmartSignals;

    /// <summary>
    /// An interface for publishing Smart Signal results
    /// </summary>
    public interface ISmartSignalResultPublisher
    {
        /// <summary>
        /// Publish Smart Signal result as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="smartSignalResult">The Smart Signal result to publish</param>
        void PublishSignalResult(string signalId, SmartSignalResult smartSignalResult);
    }
}
