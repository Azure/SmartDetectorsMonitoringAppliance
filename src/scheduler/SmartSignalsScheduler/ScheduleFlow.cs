namespace Microsoft.SmartSignals.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.SmartSignals.Scheduler.Publisher;
    using Microsoft.SmartSignals.Scheduler.SignalRunTracker;

    /// <summary>
    /// This class is responsible for discovering which signal should be executed and sends them to the analysis flow
    /// </summary>
    public class ScheduleFlow
    {
        private readonly ITracer tracer;
        private readonly ISmartSignalConfigurationStore signalConfigurationStore;
        private readonly ISignalRunsTracker signalRunsTracker;
        private readonly IAnalysisExecuter analysisExecuter;
        private readonly IDetectionPublisher detectionPublisher;
        private readonly IAzureResourceManagerClient azureResourceManagerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFlow"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="signalConfigurationStore">The signal configuration store repository</param>
        /// <param name="signalRunsTracker">The signal run tracker</param>
        /// <param name="analysisExecuter">The analysis executer instance</param>
        /// <param name="detectionPublisher">The detection publisher instance</param>
        /// <param name="azureResourceManagerClient">The azure resource manager client</param>
        public ScheduleFlow(
            ITracer tracer,
            ISmartSignalConfigurationStore signalConfigurationStore,
            ISignalRunsTracker signalRunsTracker,
            IAnalysisExecuter analysisExecuter,
            IDetectionPublisher detectionPublisher,
            IAzureResourceManagerClient azureResourceManagerClient)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.signalConfigurationStore = Diagnostics.EnsureArgumentNotNull(() => signalConfigurationStore);
            this.signalRunsTracker = Diagnostics.EnsureArgumentNotNull(() => signalRunsTracker);
            this.analysisExecuter = Diagnostics.EnsureArgumentNotNull(() => analysisExecuter);
            this.detectionPublisher = Diagnostics.EnsureArgumentNotNull(() => detectionPublisher);
            this.azureResourceManagerClient = Diagnostics.EnsureArgumentNotNull(() => azureResourceManagerClient);
        }

        /// <summary>
        /// Starting point of the schedule flow
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task RunAsync()
        {
            IList<SmartSignalConfiguration> signalConfigurations = await this.signalConfigurationStore.GetAllSmartSignalConfigurationsAsync();
            IList<SignalExecutionInfo> signalsToRun = await this.signalRunsTracker.GetSignalsToRunAsync(signalConfigurations);

            // We get all subscriptions as the resource IDs
            var resourceIds = await this.azureResourceManagerClient.GetAllSubscriptionIds();

            foreach (SignalExecutionInfo signalExecution in signalsToRun)
            {
                try
                {
                    IList<SmartSignalDetection> detections = await this.analysisExecuter.ExecuteSignalAsync(signalExecution, resourceIds);
                    this.detectionPublisher.PublishDetections(signalExecution.SignalId, detections);
                    await this.signalRunsTracker.UpdateSignalRunAsync(signalExecution);
                }
                catch (Exception exception)
                {
                    this.tracer.TraceError($"Failed executing signal {signalExecution.SignalId} with exception: {exception}");
                    this.tracer.ReportException(exception);
                }
            }
        }
    }
}
