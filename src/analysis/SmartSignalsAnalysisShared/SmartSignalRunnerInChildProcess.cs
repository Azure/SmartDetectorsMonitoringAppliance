//-----------------------------------------------------------------------
// <copyright file="SmartSignalRunnerInChildProcess.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;

    /// <summary>
    /// An implementation of <see cref="ISmartSignalRunner"/>, that runs the analysis in a separate process
    /// </summary>
    public class SmartSignalRunnerInChildProcess : ISmartSignalRunner
    {
        private const string ChildProcessName = "SmartSignalRunnerChildProcess.exe";

        private readonly IChildProcessManager childProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRunnerInChildProcess"/> class
        /// </summary>
        /// <param name="childProcessManager">The child process manager</param>
        public SmartSignalRunnerInChildProcess(IChildProcessManager childProcessManager)
        {
            this.childProcessManager = childProcessManager;
        }

        /// <summary>
        /// Runs the signal analysis, in a separate process
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the generated result items presentations</returns>
        public async Task<List<SmartSignalResultItemPresentation>> RunAsync(SmartSignalRequest request, CancellationToken cancellationToken)
        {
            return await this.childProcessManager.RunChildProcessAsync<List<SmartSignalResultItemPresentation>>(ChildProcessName, request, cancellationToken);
        }
    }
}