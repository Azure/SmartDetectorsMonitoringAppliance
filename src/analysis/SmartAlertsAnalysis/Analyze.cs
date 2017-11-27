namespace Microsoft.Azure.Monitoring.SmartAlerts.Analysis
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartAlerts.Shared.Trace;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Shared;
    using Unity;

    public static class Analyze
    {
        private static IUnityContainer _container;

        static Analyze()
        {
            _container = new UnityContainer();
        }

        [FunctionName("Analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "signals")]HttpRequestMessage request,
            TraceWriter log,
            WebJobs.ExecutionContext context,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer container = _container.CreateChildContainer())
            {
                container.RegisterInstance(TracerFactory.Create(log));
                SmartSignalRequest smartAlertRequest = await request.Content.ReadAsAsync<SmartSignalRequest>(cancellationToken);

                return request.CreateResponse(HttpStatusCode.OK, $"Received request for signal {smartAlertRequest.SignalId}");
            }
        }
    }
}
