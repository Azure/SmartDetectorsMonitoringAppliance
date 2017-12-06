namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Shared;
    using Unity;

    /// <summary>
    /// A class implementing the analysis endpoint
    /// </summary>
    public static class Analyze
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Analyze"/> class.
        /// </summary>
        static Analyze()
        {
            Container = new UnityContainer();
        }

        /// <summary>
        /// Runs the analysis flow for the requested signal.
        /// </summary>
        /// <param name="request">The request which initiated the analysis.</param>
        /// <param name="log">The Azure Function log writer.</param>
        /// <param name="context">The function's execution context.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The analysis response.</returns>
        [FunctionName("Analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "signals")]HttpRequestMessage request,
            TraceWriter log,
            WebJobs.ExecutionContext context,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer())
            {
                childContainer.RegisterInstance(TracerFactory.Create(log, true));
                SmartSignalRequest smartSignalRequest = await request.Content.ReadAsAsync<SmartSignalRequest>(cancellationToken);

                return request.CreateResponse(HttpStatusCode.OK, $"Received request for signal {smartSignalRequest.SignalId}");
            }
        }
    }
}
