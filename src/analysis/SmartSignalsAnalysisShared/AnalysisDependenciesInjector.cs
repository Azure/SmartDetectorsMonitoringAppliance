//-----------------------------------------------------------------------
// <copyright file="AnalysisDependenciesInjector.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// A static class that is used to inject dependencies related to analysis
    /// </summary>
    public static class AnalysisDependenciesInjector
    {
        /// <summary>
        /// Get a unity container with the appropriate registered dependencies, to be used for analysis
        /// </summary>
        /// <returns>The unity container</returns>
        public static IUnityContainer GetContainer()
        {
            // Register main dependencies
            IUnityContainer container = new UnityContainer();
            container
                .RegisterType<IAzureResourceManagerClient, AzureResourceManagerClient>()
                .RegisterType<ISmartSignalsRepository, SmartSignalsRepository>()
                .RegisterType<ISmartSignalAnalysisServicesFactory, SmartSignalAnalysisServicesFactory>()
                .RegisterType<ISmartSignalLoader, SmartSignalLoader>();

            return container;
        }

        /// <summary>
        /// Creates a new tracer instance and register it with the specified container
        /// </summary>
        /// <param name="container">The unity container</param>
        /// <param name="logger">The logger</param>
        /// <param name="traceToConsole">Whether to trace to console</param>
        /// <returns>The unity container, after registering the new tracer</returns>
        public static IUnityContainer WithTracer(this IUnityContainer container, TraceWriter logger, bool traceToConsole)
        {
            ITracer tracer = TracerFactory.Create(logger, traceToConsole);
            container.RegisterInstance(tracer);
            return container;
        }

        /// <summary>
        /// Registers the specified type with the <see cref="ISmartSignalRunner"/> interface.
        /// </summary>
        /// <typeparam name="TSmartSignalRunner">The smart signal runner type</typeparam>
        /// <param name="container">The unity container</param>
        /// <returns>The unity container, after registering the smart signal runner type</returns>
        public static IUnityContainer WithSmartSignalRunner<TSmartSignalRunner>(this IUnityContainer container) where TSmartSignalRunner : ISmartSignalRunner
        {
            container.RegisterType<ISmartSignalRunner, TSmartSignalRunner>();
            return container;
        }
    }
}