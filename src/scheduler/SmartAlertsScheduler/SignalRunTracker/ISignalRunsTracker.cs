namespace Microsoft.SmartSignals.Scheduler.SignalRunTracker
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Monitoring.SmartAlerts.Shared;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public interface ISignalRunsTracker
    {
        /// <summary>
        /// Gets the IDs of the signal that needs to be executed based on configuration and their last executation times
        /// </summary>
        /// <param name="signalConfigurations">list of signal configurations</param>
        /// <returns>The signal IDs</returns>
        Task<IList<string>> GetSignalsToRunAsync(IEnumerable<SmartSignalConfiguration> signalConfigurations);

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="signalId">The signal ID of the signal to update</param>
        Task UpdateSignalRunAsync(string signalId);
    }
}
