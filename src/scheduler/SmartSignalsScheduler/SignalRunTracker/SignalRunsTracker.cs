namespace Microsoft.SmartSignals.Scheduler.SignalRunTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public class SignalRunsTracker : ISignalRunsTracker
    {
        private const string TableName = "signaltracking";
        private const string PartitionKey = "tracking";

        private readonly ICloudTableWrapper trackingTable;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the<see cref="SignalRunsTracker"/> class.
        /// </summary>
        /// <param name="tableClient">The azure storage table client</param>
        /// <param name="tracer">Log wrapper</param>
        public SignalRunsTracker(ICloudTableClientWrapper tableClient, ITracer tracer)
        {
            this.tracer = tracer;

            // create the cloud table instance
            this.trackingTable = tableClient.GetTableReference(TableName);
            this.trackingTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the configurations of the signal that needs to be executed based on configuration and their last execution times
        /// </summary>
        /// <param name="signalConfigurations">list of signal configurations</param>
        /// <returns>A list of signal execution times of the signals to execute</returns>
        public async Task<IList<SignalExecutionInfo>> GetSignalsToRunAsync(IEnumerable<SmartSignalConfiguration> signalConfigurations)
        {
            this.tracer.TraceVerbose("getting signals to run");

            // get all last signal runs from table storage
            var signalsLastRuns = await this.trackingTable.ReadPartitionAsync<TrackSignalRunEntity>(PartitionKey);

            // create a dictionary from signal ID to signal execution for faster lookup
            var signalIdToLastRun = signalsLastRuns.ToDictionary(x => x.RowKey, x => x);

            // for each signal check if needs to be run based on its schedule and its last execution time
            var signalsToRun = new List<SignalExecutionInfo>();
            foreach (var signalConfiguration in signalConfigurations)
            {
                bool signalWasExecutedBefore = signalIdToLastRun.TryGetValue(signalConfiguration.SignalId, out var signalLastRun);
                var nextBaseTime = signalWasExecutedBefore ? signalLastRun.LastSuccessfulRunEndTime : DateTime.MinValue;
                DateTime signalNextRun = signalConfiguration.Schedule.GetNextOccurrence(nextBaseTime);
                if (signalNextRun <= DateTime.UtcNow)
                {
                    this.tracer.TraceInformation($"signal {signalConfiguration.SignalId} last ran at {signalLastRun} and is marked to run");
                    signalsToRun.Add(this.GenerateSignalExecutionFromConfiguration(signalConfiguration, signalLastRun?.LastSuccessfulRunEndTime));
                }
            }

            return signalsToRun;
        }

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="signalExecutionInfo">The current signal execution information</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation</returns>
        public async Task UpdateSignalRunAsync(SignalExecutionInfo signalExecutionInfo)
        {
            // Execute the update operation
            this.tracer.TraceVerbose($"updating run for signal: {signalExecutionInfo.SignalId}");
            var operation = TableOperation.InsertOrReplace(new TrackSignalRunEntity
            {
                PartitionKey = PartitionKey,
                RowKey = signalExecutionInfo.SignalId,
                LastSuccessfulRunStartTime = signalExecutionInfo.AnalysisStartTime,
                LastSuccessfulRunEndTime = signalExecutionInfo.AnalysisEndTime
            });
            await this.trackingTable.ExecuteAsync(operation);
        }

        /// <summary>
        /// Generates signal execution details from configuration and the last analysis execution run time
        /// </summary>
        /// <param name="signalConfiguration">The smart signal configuration</param>
        /// <param name="lastAnalysisEndTime">The end time of the last analysis execution</param>
        /// <returns>The signal execution details.</returns>
        private SignalExecutionInfo GenerateSignalExecutionFromConfiguration(SmartSignalConfiguration signalConfiguration, DateTime? lastAnalysisEndTime = null)
        {
            // Get the window size and analysis timestamp based on the predefined CRON schedule and the last analysis time
            DateTime currentRunOccurrence;
            DateTime previousRunOccurrence;
            if (lastAnalysisEndTime == null)
            {
                // First time run - We want to get the latest possible run time based on the CRON schedule; e.g. for CRON that runs every round hour, if UtcNow is 03:07 then next run should be 03:00.
                // Since we don't have a the last successful run we take a time from distant past and from all possible execution we take the last one.
                var distantPast = DateTime.UtcNow.AddMonths(-1);
                currentRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences(distantPast, DateTime.UtcNow).Last();
                previousRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences(distantPast, currentRunOccurrence).Last();
            }
            else
            {
                // We want to get the latest possible run time based on the CRON schedule; e.g. for CRON that runs every round hour, if UtcNow is 03:07 then next run should be 03:00.
                // We take the last successful run and from that time we get all possible execution and choose the last possible one.
                currentRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences((DateTime)lastAnalysisEndTime, DateTime.UtcNow).Last();
                var possiblePreviousOccurrences = signalConfiguration.Schedule.GetNextOccurrences((DateTime)lastAnalysisEndTime, currentRunOccurrence).ToList();
                previousRunOccurrence = possiblePreviousOccurrences.Any() ? possiblePreviousOccurrences.Last() : (DateTime)lastAnalysisEndTime;
            }

            return new SignalExecutionInfo
            {
                SignalId = signalConfiguration.SignalId,
                AnalysisStartTime = previousRunOccurrence,
                AnalysisEndTime = currentRunOccurrence
            };
        }
    }
}
