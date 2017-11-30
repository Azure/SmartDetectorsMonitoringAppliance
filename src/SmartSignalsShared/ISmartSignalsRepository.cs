namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the smart signals repository
    /// </summary>
    public interface ISmartSignalsRepository
    {
        /// <summary>
        /// Reads a smart signal's metadata from the repository
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the smart signal metadata</returns>
        Task<SmartSignalMetadata> ReadSignalMetadataAsync(string signalId);
    }
}