namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This interface represents the /detections API logic.
    /// </summary>
    public interface IDetectionsLogic
    {
        /// <summary>
        /// Gets all the detections.
        /// </summary>
        /// <returns>The smart detections.</returns>
        Task<IEnumerable<SmartSignalDetection>> GetAllDetectionsAsync();
    }
}
