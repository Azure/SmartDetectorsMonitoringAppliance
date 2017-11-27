namespace Microsoft.SmartAlerts.Analysis
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Shared;

    public static class Analyze
    {
        [FunctionName("Analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "signals/{signalId}")]HttpRequestMessage request,
            string signalId,
            TraceWriter log)
        {
            SmartAlertRequest smartAlertRequest = await request.Content.ReadAsAsync<SmartAlertRequest>();
            smartAlertRequest.SignalId = signalId;

            return request.CreateResponse(HttpStatusCode.OK, $"Recieved request for signal {smartAlertRequest.SignalId}");
        }
    }
}
