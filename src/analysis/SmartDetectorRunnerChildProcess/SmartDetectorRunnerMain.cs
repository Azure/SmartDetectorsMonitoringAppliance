//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerMain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorRunnerChildProcess
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Unity;

    /// <summary>
    /// The main class of the process that runs the Smart Detector
    /// </summary>
    public static class SmartDetectorRunnerMain
    {
        private static IUnityContainer container;

        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">Command line arguments. These arguments are expected to be created by <see cref="IChildProcessManager.RunChildProcessAsync{TOutput}"/>.</param>
        /// <returns>Exit code</returns>
        public static int Main(string[] args)
        {
            IExtendedTracer tracer = null;
            try
            {
                // Inject dependencies
                container = DependenciesInjector.GetContainer()
                    .InjectAnalysisDependencies(withChildProcessRunner: false)
                    .WithChildProcessRegistrations(args);

                // Trace
                tracer = container.Resolve<IExtendedTracer>();
                tracer.TraceInformation($"Starting Smart Detector runner process, process ID {Process.GetCurrentProcess().Id}");

                // Run the analysis
                IChildProcessManager childProcessManager = container.Resolve<IChildProcessManager>();
                return childProcessManager.RunAndListenToParentAsync<SmartDetectorRunnerChildProcessInput, object>(args, RunSmartDetectorAsync, ConvertExceptionToExitCode).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                tracer?.ReportException(e);
                tracer?.TraceError("Unhandled exception in child process: " + e.Message);
                Console.Error.WriteLine(e.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Run the Smart Detector, by delegating the call to the registered <see cref="ISmartDetectorRunner"/>
        /// </summary>
        /// <param name="request">The Smart Detector request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the result of the call to the runner.</returns>
        private static async Task<object> RunSmartDetectorAsync(SmartDetectorRunnerChildProcessInput request, CancellationToken cancellationToken)
        {
            ISmartDetectorRunner smartDetectorRunner = container.Resolve<ISmartDetectorRunner>();
            bool shouldDetectorTrace = bool.Parse(ConfigurationReader.ReadConfig("ShouldDetectorTrace", required: true));

            if (request.AnalysisRequest != null)
            {
                return await smartDetectorRunner.AnalyzeAsync(request.AnalysisRequest, shouldDetectorTrace, cancellationToken);
            }
            else if (request.AlertResolutionCheckRequest != null)
            {
                return await smartDetectorRunner.CheckResolutionAsync(request.AlertResolutionCheckRequest, shouldDetectorTrace, cancellationToken);
            }

            throw new ArgumentException("Unable to determine flow to run for Smart Detector", nameof(request));
        }

        /// <summary>
        /// Converts an exception that was thrown from running a Smart Detector to the process's exit code
        /// </summary>
        /// <param name="e">The exception to convert</param>
        /// <returns>The process's exit code</returns>
        private static int ConvertExceptionToExitCode(Exception e)
        {
            switch (e)
            {
                case SmartDetectorNotFoundException _:
                    return (int)HttpStatusCode.NotFound;

                case SmartDetectorLoadException _:
                    return (int)HttpStatusCode.NotFound;

                case IncompatibleResourceTypesException _:
                    return (int)HttpStatusCode.BadRequest;

                case AzureResourceManagerClientException armce when armce.StatusCode.HasValue:
                    return (int)armce.StatusCode;

                case UnidentifiedAlertResourceTypeException _:
                    return (int)HttpStatusCode.BadRequest;

                case ResolutionCheckNotSupportedException _:
                    return (int)HttpStatusCode.BadRequest;

                case ResolutionStateNotFoundException _:
                    return (int)HttpStatusCode.NotFound;

                default:
                    return (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
