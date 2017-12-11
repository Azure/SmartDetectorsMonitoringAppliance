namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;

    /// <summary>
    /// Interface to load a signal based on its ID
    /// </summary>
    public interface ISmartSignalLoader
    {
        /// <summary>
        /// Load a signal based on its metadata
        /// </summary>
        /// <param name="signalMetadata">The signal metadata</param>
        /// <returns>The signal instance</returns>
        Task<ISmartSignal> LoadSignalAsync(SmartSignalMetadata signalMetadata);
    }
}