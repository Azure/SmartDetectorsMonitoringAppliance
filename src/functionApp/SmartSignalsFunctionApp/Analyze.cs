//-----------------------------------------------------------------------
// <copyright file="Analyze.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using Unity;
    using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

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
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<IAnalysisServicesFactory, AnalysisServicesFactory>()
                .RegisterType<ISmartSignalLoader, SmartSignalLoader>()
                .RegisterType<ISmartSignalRunner, SmartSignalRunnerInChildProcess>();
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage request,
            TraceWriter log,
            ExecutionContext context,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                // Create a tracer for this run (that will also log to the specified TraceWriter)
                ITracer tracer = childContainer.Resolve<ITracer>();
                tracer.TraceInformation($"Analyze function request received with invocation Id {context.InvocationId}");
                tracer.AddCustomProperty("FunctionName", context.FunctionName);
                tracer.AddCustomProperty("InvocationId", context.InvocationId.ToString("N"));

                try
                {
                    // Read the request
                    SmartSignalRequest smartSignalRequest = await request.Content.ReadAsAsync<SmartSignalRequest>(cancellationToken);
                    tracer.AddCustomProperty("SignalId", smartSignalRequest.SignalId);
                    tracer.TraceInformation($"Analyze request received: {JsonConvert.SerializeObject(smartSignalRequest)}");

                    // Process the request
                    ISmartSignalRunner runner = childContainer.Resolve<ISmartSignalRunner>();
                    List<SmartSignalResultItemPresentation> resultPresentations = await runner.RunAsync(smartSignalRequest, cancellationToken);
                    tracer.TraceInformation($"Analyze completed, returning {resultPresentations.Count} results");

                    // Return the generated presentations
                    return request.CreateResponse(HttpStatusCode.OK, resultPresentations);
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
