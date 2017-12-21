namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using WebJobs;
    using WebJobs.Extensions.Http;
    using WebJobs.Host;

    /// <summary>
    /// This class is the entry point for the /detections endpoint.
    /// </summary>
    public static class Detections
    {
        /// <summary>
        /// Gets all the detections.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <returns>The detections.</returns>
        [FunctionName("v1/Detections")]
        public static async Task<HttpResponseMessage> GetAllDetections([HttpTrigger(AuthorizationLevel.Function, "get")]HttpRequestMessage req, TraceWriter log)
        {
            await Task.CompletedTask;

            return req.CreateResponse();
        }
    }
}
