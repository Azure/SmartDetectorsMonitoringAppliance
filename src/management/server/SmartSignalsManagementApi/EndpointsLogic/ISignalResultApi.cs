namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This class is the logic for the /signalResult endpoint.
    /// </summary>
    public interface ISignalResultApi
    {
        /// <summary>
        /// Gets all the detections.
        /// </summary>
        /// <returns>The smart detections.</returns>
        Task<IEnumerable<SmartSignalDetection>> GetAllDetectionsAsync();
    }
}
