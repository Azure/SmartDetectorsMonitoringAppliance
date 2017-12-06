namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface providing methods for retrieving and saving a smart signal configuration.
    /// </summary>
    public interface ISmartSignalConfigurationStore
    {
        /// <summary>
        /// Gets all the signal configurations from the store.
        /// </summary>
        /// <returns>A <see cref="IList{SmartSignalConfiguration}"/> containing all the signal configurations in the store.</returns>
        Task<IList<SmartSignalConfiguration>> GetAllSmartSignalConfigurationsAsync();

        /// <summary>
        /// Adds or updates a signal configuration in the store.
        /// </summary>
        /// <param name="signalConfiguration">The signal configuration to add to the store.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        Task AddOrReplaceSmartSignalConfigurationAsync(SmartSignalConfiguration signalConfiguration);
    }
}
