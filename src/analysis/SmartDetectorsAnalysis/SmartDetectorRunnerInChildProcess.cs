//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerInChildProcess.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    /// <summary>
    /// An implementation of <see cref="ISmartDetectorRunner"/>, that runs the analysis in a separate process
    /// </summary>
    public class SmartDetectorRunnerInChildProcess : ISmartDetectorRunner
    {
        private const string ChildProcessName = "SmartDetectorRunnerChildProcess.exe";

        private readonly IChildProcessManager childProcessManager;
        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunnerInChildProcess"/> class
        /// </summary>
        /// <param name="childProcessManager">The child process manager</param>
        /// <param name="tracer">The tracer</param>
        public SmartDetectorRunnerInChildProcess(IChildProcessManager childProcessManager, IExtendedTracer tracer)
        {
            this.childProcessManager = childProcessManager;
            this.tracer = tracer;
        }

        /// <summary>
        /// Runs the Smart Detector analysis, in a separate process
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="shouldDetectorTrace">Determines if the detector's traces are emitted</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the generated alerts presentations</returns>
        public async Task<List<ContractsAlert>> RunAsync(SmartDetectorAnalysisRequest request, bool shouldDetectorTrace, CancellationToken cancellationToken)
        {
            // Find the executable location
            string currentDllPath = new Uri(typeof(SmartDetectorRunnerInChildProcess).Assembly.CodeBase).AbsolutePath;
            string exePath = Path.Combine(Path.GetDirectoryName(currentDllPath) ?? string.Empty, ChildProcessName);
            if (!File.Exists(exePath))
            {
                this.tracer.TraceError($"Verification of executable path {exePath} failed");
                throw new FileNotFoundException("Could not find child process executable", ChildProcessName);
            }

            try
            {
                // Run the child process
                return await this.childProcessManager.RunChildProcessAsync<List<ContractsAlert>>(exePath, request, cancellationToken);
            }
            catch (ChildProcessFailedException e)
            {
                if (Enum.IsDefined(typeof(HttpStatusCode), e.ExitCode))
                {
                    throw new AnalysisFailedException((HttpStatusCode)e.ExitCode, e.Message, e);
                }

                throw new AnalysisFailedException($"Running Smart Detector analysis in child process failed with exit code {e.ExitCode} and message: {e.Message}", e);
            }
        }
    }
}