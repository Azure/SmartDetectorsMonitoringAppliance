//-----------------------------------------------------------------------
// <copyright file="SmartDetector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Security;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using Unity;
    using SmartDetectorResponse = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.SmartDetector;

    /// <summary>
    /// This class is the entry point for the Smart Detector endpoint.
    /// </summary>
    public static class SmartDetector
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="SmartDetector"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "This must be called at initialization")]
        static SmartDetector()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            // Force use the most updated TLS protocol
            SecurityProtocol.RemoveUnsecureProtocols();

            Container = DependenciesInjector.GetContainer()
                .RegisterType<ISmartDetectorApi, SmartDetectorApi>();
        }

        /// <summary>
        /// Gets all the Smart Detectors.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The Smart Detectors encoded as JSON.</returns>
        [FunctionName("GetSmartDetectors")]
        public static async Task<HttpResponseMessage> GetAllSmartDetectors(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "smartDetector")] HttpRequestMessage req,
            TraceWriter log,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var smartDetectorApi = childContainer.Resolve<ISmartDetectorApi>();

                try
                {
                    ListSmartDetectorsResponse smartDetectors = await smartDetectorApi.GetSmartDetectorsAsync(cancellationToken);

                    // Create the response with StringContent to prevent Json from serializing to a string
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(smartDetectors), Encoding.UTF8, "application/json");
                    return response;
                }
                catch (SmartDetectorsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get Smart Detectors due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get Smart Detectors", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get Smart Detectors due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get Smart Detectors", e);
                }
            }
        }

        /// <summary>
        /// Gets all the Smart Detectors.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="detector">The detector ID</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The Smart Detectors encoded as JSON.</returns>
        [FunctionName("GetSmartDetector")]
        public static async Task<HttpResponseMessage> GetSmartDetector(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "smartDetector/{detector}")] HttpRequestMessage req,
            string detector,
            TraceWriter log,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var smartDetectorApi = childContainer.Resolve<ISmartDetectorApi>();

                try
                {
                    SmartDetectorResponse smartDetector = await smartDetectorApi.GetSmartDetectorAsync(detector, cancellationToken);

                    // Create the response with StringContent to prevent Json from serializing to a string
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(smartDetector), Encoding.UTF8, "application/json");
                    return response;
                }
                catch (SmartDetectorsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get Smart Detector due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get Smart Detector", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get Smart Detector due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get Smart Detector", e);
                }
            }
        }
    }
}
