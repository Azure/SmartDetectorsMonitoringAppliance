//-----------------------------------------------------------------------
// <copyright file="ISignalRunsTracker.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.SignalRunTracker
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AlertRules;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public interface ISignalRunsTracker
    {
        /// <summary>
        /// Returns the execution information of the signals that needs to be executed based on their last execution times and the alert rules
        /// </summary>
        /// <param name="alertRules">The alert rules</param>
        /// <returns>A list of signal execution times of the signals to execute</returns>
        Task<IList<SignalExecutionInfo>> GetSignalsToRunAsync(IEnumerable<AlertRule> alertRules);

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="signalExecutionInfo">The current signal execution information</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation</returns>
        Task UpdateSignalRunAsync(SignalExecutionInfo signalExecutionInfo);
    }
}
