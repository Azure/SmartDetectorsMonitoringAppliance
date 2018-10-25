//-----------------------------------------------------------------------
// <copyright file="Analyze.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Security;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using Unity;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
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
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "This must be called at initialization")]
        static Analyze()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            // Force use the most updated TLS protocol
            SecurityProtocol.RemoveUnsecureProtocols();

            Container = DependenciesInjector.GetContainer()
                .InjectAnalysisDependencies(withChildProcessRunner: true);
        }

        /// <summary>
        /// Runs the analysis flow for the requested Smart Detector.
        /// </summary>
        /// <param name="request">The request which initiated the analysis.</param>
        /// <param name="log">The Azure Function log writer.</param>
        /// <param name="context">The function's execution context.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The analysis response.</returns>
        [FunctionName("Analyze")]
        public static async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "analyze")]HttpRequestMessage request,
            TraceWriter log,
            ExecutionContext context,
            CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                // Create a tracer for this run (that will also log to the specified TraceWriter)
                IExtendedTracer tracer = childContainer.Resolve<IExtendedTracer>();
                tracer.TraceInformation($"Analyze function request received with invocation Id {context.InvocationId}");
                tracer.AddCustomProperty("FunctionName", context.FunctionName);
                tracer.AddCustomProperty("InvocationId", context.InvocationId.ToString("N", CultureInfo.InvariantCulture));

                try
                {
                    // Trace app counters (before analysis)
                    tracer.TraceAppCounters();

                    // Read the request
                    SmartDetectorExecutionRequest smartDetectorExecutionRequest = await request.Content.ReadAsAsync<SmartDetectorExecutionRequest>(cancellationToken);
                    tracer.AddCustomProperty("SmartDetectorId", smartDetectorExecutionRequest.SmartDetectorId);
                    tracer.TraceInformation($"Analyze request received: {JsonConvert.SerializeObject(smartDetectorExecutionRequest)}");

                    // Process the request
                    ISmartDetectorRunner runner = childContainer.Resolve<ISmartDetectorRunner>();
                    bool shouldDetectorTrace = bool.Parse(ConfigurationReader.ReadConfig("ShouldDetectorTrace", required: true));
                    List<ContractsAlert> alertPresentations = await runner.RunAsync(smartDetectorExecutionRequest, shouldDetectorTrace, cancellationToken);
                    tracer.TraceInformation($"Analyze completed, returning {alertPresentations.Count} Alerts");

                    // Create the response with StringContent to prevent Json from serializing to a string
                    var response = request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(alertPresentations), Encoding.UTF8, "application/json");
                    return response;
                }
                catch (AnalysisFailedException afe)
                {
                    // Handle the exception
                    TopLevelExceptionHandler.TraceUnhandledException(afe, tracer, log);

                    // Return error status
                    return request.CreateResponse(afe.StatusCode, afe.ReasonPhrase);
                }
                catch (Exception e)
                {
                    // Handle the exception
                    TopLevelExceptionHandler.TraceUnhandledException(e, tracer, log);

                    // Return error status
                    return request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                finally
                {
                    // Trace app counters (after analysis)
                    tracer.TraceAppCounters();
                }
            }
        }
    }
}
