namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Threading.Tasks;
    using Responses;
    using Shared.Models;

    /// <summary>
    /// This interface represents the /signals API logic.
    /// </summary>
    public interface ISignalsLogic
    {
        /// <summary>
        /// Gets all the smart signals.
        /// </summary>
        /// <returns>The smart signals.</returns>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to retrieve smart signals.</exception>
        Task<ListSmartSignalsResponse> GetAllSmartSignalsAsync();

        /// <summary>
        /// Add the given signal to the smart signal configuration store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addSignalVersion">The model that contains all the require parameters for adding signal version.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add smart signals configurations.</exception>
        Task AddSignalVersionAsync(AddSignalVersion addSignalVersion);
    }
}
