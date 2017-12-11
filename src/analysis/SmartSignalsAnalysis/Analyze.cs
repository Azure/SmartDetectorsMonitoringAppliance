namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis.DetectionPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
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
            Container = AnalysisDependenciesInjector.GetContainer()
                .WithSmartSignalRunner<SmartSignalRunnerInChildProcess>();
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
        public static async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "signals")]HttpRequestMessage request,
            TraceWriter log,
            WebJobs.ExecutionContext context,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                // Create a tracer for this run (that will also log to the specified TraceWriter)
                ITracer tracer = childContainer.Resolve<ITracer>();
                tracer.TraceInformation($"Analyze function request received with invocation Id {context.InvocationId}");

                try
                {
                    // Read the request
                    SmartSignalRequest smartSignalRequest = await request.Content.ReadAsAsync<SmartSignalRequest>(cancellationToken);

                    // Process the request
                    ISmartSignalRunner runner = childContainer.Resolve<ISmartSignalRunner>();
                    List<SmartSignalDetectionPresentation> detections = await runner.RunAsync(smartSignalRequest, cancellationToken);

                    // Return the generated detections
                    return request.CreateResponse(HttpStatusCode.OK, detections);
                }
                catch (Exception e)
                {
                    // Handle the exception
                    TopLevelExceptionHandler.TraceUnhandledException(e, tracer, log);

                    // Return error status
                    return request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
            }
        }
    }
}
