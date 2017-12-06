namespace Microsoft.SmartSignals.Scheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.SmartSignals.Scheduler.Publisher;
    using Microsoft.SmartSignals.Scheduler.SignalRunTracker;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Unity;

    /// <summary>
    /// A class responsible for scheduling the signals execution
    /// </summary>
    public static class Schedule
    {
        private static readonly IUnityContainer _container;

        /// <summary>
        /// Initializes static members of the <see cref="Schedule"/> class.
        /// </summary>
        static Schedule()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            // TODO: get storage connection string from KV
            var storageConnectionString = "";
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();

            _container = new UnityContainer()
                .RegisterInstance(cloudTableClient)
                .RegisterType<ICloudTableClientWrapper, CloudTableClientWrapper>()
                .RegisterType<ISmartSignalConfigurationStore, SmartSignalConfigurationStore>()
                .RegisterType<ISignalRunsTracker, SignalRunsTracker>()
                .RegisterType<IAnalysisExecuter, AnalysisExecuter>()
                .RegisterType<IDetectionPublisher, DetectionPublisher>();
        }

        /// <summary>
        /// The starting point for the smart signal scheduler
        /// </summary>
        /// <param name="myTimer">The timer information of the timer trigger used by Azure to trigger the function</param>
        /// <param name="log">The function's logger</param>
        [FunctionName("Schedule")]
        public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            using (IUnityContainer childContainer = _container.CreateChildContainer())
            {
                // Since we add the web job log tracer for each function invocation then we need to register the instance here in a child container
                var tracer = TracerFactory.Create(log, true);
                childContainer.RegisterInstance(tracer);
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
                    tracer.ReportException(exception);
                    throw;
                }
            }
        }
    }
}
