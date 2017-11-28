namespace Microsoft.SmartSignals.Scheduler
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SmartAlerts.Shared;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public interface ISignalRunsTracker
    {
        Task<IEnumerable<string>> GetSignalsToRunAsync(IEnumerable<SignalConfiguration> signalConfigurations);

        Task UpdateSignalRun(string signalId);
    }
}
