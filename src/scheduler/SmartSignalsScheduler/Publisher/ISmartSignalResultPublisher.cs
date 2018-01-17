//-----------------------------------------------------------------------
// <copyright file="ISmartSignalResultPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.SignalResultPresentation;

    /// <summary>
    /// An interface for publishing Smart Signal results
    /// </summary>
    public interface ISmartSignalResultPublisher
    {
        /// <summary>
        /// Publish Smart Signal result items as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="smartSignalResultItems">The Smart Signal result items to publish</param>
        void PublishSignalResultItems(string signalId, IList<SmartSignalResultItemPresentation> smartSignalResultItems);
    }
}
