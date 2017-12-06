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
        private readonly ITracer _tracer;
        private readonly ISmartSignalConfigurationStore _signalConfigurationStore;
        private readonly ISignalRunsTracker _signalRunsTracker;
        private readonly IAnalysisExecuter _analysisExecuter;
        private readonly IDetectionPublisher _detectionPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFlow"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="signalConfigurationStore">The signal configuration store repository</param>
        /// <param name="signalRunsTracker">The signal run tracker</param>
        /// <param name="analysisExecuter">The analysis executer instance</param>
        /// <param name="detectionPublisher">The detection publisher instance</param>
        public ScheduleFlow(ITracer tracer, ISmartSignalConfigurationStore signalConfigurationStore, ISignalRunsTracker signalRunsTracker, IAnalysisExecuter analysisExecuter, IDetectionPublisher detectionPublisher)
        {
            _tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            _signalConfigurationStore = Diagnostics.EnsureArgumentNotNull(() => signalConfigurationStore);
            _signalRunsTracker = Diagnostics.EnsureArgumentNotNull(() => signalRunsTracker);
            _analysisExecuter = Diagnostics.EnsureArgumentNotNull(() => analysisExecuter);
            _detectionPublisher = Diagnostics.EnsureArgumentNotNull(() => detectionPublisher);
        }

        /// <summary>
        /// Starting point of the schedule flow
        /// </summary>
        public async Task RunAsync()
        {
            IList<SmartSignalConfiguration> signalConfigurations = await _signalConfigurationStore.GetAllSmartSignalConfigurationsAsync();
            IList<SignalExecutionInfo> signalsToRun = await _signalRunsTracker.GetSignalsToRunAsync(signalConfigurations);

            // TODO: get resources
            var resourceIds = new List<string>();

            foreach (SignalExecutionInfo signalExecution in signalsToRun)
            {
                try
                {
                    IList<SmartSignalDetection> detections = await _analysisExecuter.ExecuteSignalAsync(signalExecution, resourceIds);
                    _detectionPublisher.PublishDetections(signalExecution.SignalId, detections);
                    await _signalRunsTracker.UpdateSignalRunAsync(signalExecution);
                }
                catch (Exception exception)
                {
                    _tracer.TraceError($"Failed executing signal {signalExecution.SignalId} with exception: {exception}");
                    _tracer.ReportException(exception);
                }
            }
        }
    }
}
