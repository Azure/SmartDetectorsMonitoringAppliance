namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Shared.DetectionPresentation;

    /// <summary>
    /// This class is the entry point for the /detections endpoint.
    /// </summary>
    public class DetectionsController : ApiController
    {
        /// <summary>
        /// Gets all the detections.
        /// </summary>
        /// <returns>The detections.</returns>
        [HttpGet]
        [Route("api/v1/detections")]
        public Task<IEnumerable<IList<SmartSignalDetectionPresentation>>> Get()
        {
            return null;
        }
    }
}
