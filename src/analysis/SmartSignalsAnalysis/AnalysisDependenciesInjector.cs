//-----------------------------------------------------------------------
// <copyright file="AnalysisDependenciesInjector.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Unity;

    /// <summary>
    /// A static class that is used to inject dependencies of analysis classes
    /// </summary>
    public static class AnalysisDependenciesInjector
    {
        /// <summary>
        /// Registers common analysis dependencies to the specified unity container
        /// </summary>
        /// <param name="container">The unity container</param>
        /// <param name="withChildProcessRunner">Whether to run signals in a child process</param>
        /// <returns>The unity container, after registering the analysis dependencies</returns>
        public static IUnityContainer InjectAnalysisDependencies(this IUnityContainer container, bool withChildProcessRunner)
        {
            container = container
                .RegisterType<IAnalysisServicesFactory, AnalysisServicesFactory>()
                .RegisterType<ISmartSignalLoader, SmartSignalLoader>();

            if (withChildProcessRunner)
            {
                container = container.RegisterType<ISmartSignalRunner, SmartSignalRunnerInChildProcess>();
            }
            else
            {
                container = container.RegisterType<ISmartSignalRunner, SmartSignalRunner>();
            }

            return container;
        }

        /// <summary>
        /// Registers a tracer instance for the child process, based on the arguments received from the parent process.
        /// </summary>
        /// <param name="container">The unity container</param>
        /// <param name="args">The command line arguments</param>
        /// <returns>The unity container, after registering the tracer instance</returns>
        public static IUnityContainer WithChildProcessTracer(this IUnityContainer container, string[] args)
        {
            // We need to use an instance of IChildProcessManager, just to create the tracer.
            // To overcome the "chicken and egg" problem, we use a child container.
            ITracer tracer;
            using (IUnityContainer childContainer = container.CreateChildContainer())
            {
                ITracer childTracer = new ConsoleTracer(string.Empty);
                childContainer.RegisterInstance(childTracer);
                IChildProcessManager childProcessManager = childContainer.Resolve<IChildProcessManager>();
                tracer = childProcessManager.CreateTracerForChildProcess(args);
            }

            // Now register the instance with the parent container
            container.RegisterInstance(tracer);
            return container;
        }
    }
}
