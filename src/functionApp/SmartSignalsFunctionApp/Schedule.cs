﻿namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.SmartSignals.Scheduler;
    using Microsoft.SmartSignals.Scheduler.Publisher;
    using Microsoft.SmartSignals.Scheduler.SignalRunTracker;
    using Shared;
    using Shared.AzureStorage;
    using Shared.SignalConfiguration;
    using SmartSignals;
    using Unity;
    using WebJobs;
    using WebJobs.Host;
    using WindowsAzure.Storage;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// A class responsible for scheduling the signals execution
    /// </summary>
    public static class Schedule
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Schedule"/> class.
        /// </summary>
        static Schedule()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();

            Container = new UnityContainer()
                .RegisterInstance(cloudTableClient)
                .RegisterType<ICloudTableClientWrapper, CloudTableClientWrapper>()
                .RegisterType<ISmartSignalConfigurationStore, SmartSignalConfigurationStore>()
                .RegisterType<ISignalRunsTracker, SignalRunsTracker>()
                .RegisterType<IAnalysisExecuter, AnalysisExecuter>()
                .RegisterType<IDetectionPublisher, DetectionPublisher>()
                .RegisterType<IAzureResourceManagerClient, AzureResourceManagerClient>();
        }

        /// <summary>
        /// The starting point for the smart signal scheduler
        /// </summary>
        /// <param name="myTimer">The timer information of the timer trigger used by Azure to trigger the function</param>
        /// <param name="log">The function's logger</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        [FunctionName("Schedule")]
        public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // Since we add the web job log tracer for each function invocation then we need to register the instance here in a child container
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var scheduleFlow = childContainer.Resolve<ScheduleFlow>();

                try
                {
                    tracer.TraceInformation($"Executing schedule flow");
                    await scheduleFlow.RunAsync();
                    tracer.TraceInformation($"Executed schedule flow successfully");
                }
                catch (Exception exception)
                {
                    tracer.TraceError($"Failed running scheduling with exception {exception}");
                    TopLevelExceptionHandler.TraceUnhandledException(exception, tracer, log);
                    throw;
                }
            }
        }
    }
}
