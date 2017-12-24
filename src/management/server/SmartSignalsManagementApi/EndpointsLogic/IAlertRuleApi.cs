namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Models;

    /// <summary>
    /// This class is the logic for the /alertRule endpoint.
    /// </summary>
    public interface IAlertRuleApi
    {
        /// <summary>
        /// Add the given alert rule to the alert rules store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addAlertRule">The model that contains all the require parameters for adding signal version.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add smart signals version.</exception>
        Task AddAlertRuleAsync(AddAlertRule addAlertRule);
    }
}
