namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Analysis;
    using ManagementApi;
    using ManagementApi.EndpointsLogic;
    using ManagementApi.Responses;
    using Shared;
    using Shared.Models;
    using Shared.SignalConfiguration;
    using Unity;
    using WebJobs;
    using WebJobs.Extensions.Http;
    using WebJobs.Host;

    /// <summary>
    /// This class is the entry point for the /signals endpoint.
    /// </summary>
    public static class Signals
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Signals"/> class.
        /// </summary>
        static Signals()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = new UnityContainer()
                .RegisterType<ISmartSignalConfigurationStore, SmartSignalConfigurationStore>()
                .RegisterType<ISignalsLogic, SignalsLogic>();
        }

        /// <summary>
        /// Gets all the smart signals.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <returns>The smart signals encoded as JSON.</returns>
        [FunctionName("Signals/query")]
        public static async Task<HttpResponseMessage> GetAllSmartSignals([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestMessage req, TraceWriter log)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var signalsLogic = childContainer.Resolve<SignalsLogic>();

                try
                {
                    ListSmartSignalsResponse smartSignals = await signalsLogic.GetAllSmartSignalsAsync();

                    return req.CreateResponse(smartSignals);
                }
                catch (SmartSignalsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get smart signals due of managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get smart signals", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get smart signals due of un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get smart signals", e);
                }
            }
        }

        /// <summary>
        /// Add the given signal to the smart signal configuration store.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <returns>200 if request was successful, 500 if not.</returns>
        [FunctionName("v1/Signals")]
        public static async Task<HttpResponseMessage> AddSignalVersion([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestMessage req, TraceWriter log)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var signalsLogic = childContainer.Resolve<SignalsLogic>();

                // Read given parameters from body
                var addSignalVersion = await req.Content.ReadAsAsync<AddSignalVersion>();

                try
                {
                    await signalsLogic.AddSignalVersionAsync(addSignalVersion);

                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch (SmartSignalsManagementApiException e)
                {
                    tracer.TraceError($"Failed to add smart signal configuration due of managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to add the given smart signal configuration", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to add smart signal configuration due of un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get smart signals", e);
                }
            }
        }
    }
}
