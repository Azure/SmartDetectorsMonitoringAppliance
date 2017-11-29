namespace Microsoft.SmartSignals.Scheduler.SignalRunTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using WindowsAzure.Storage;
    using WindowsAzure.Storage.RetryPolicies;
    using WindowsAzure.Storage.Table;
    using Azure.Monitoring.SmartAlerts.Shared;
    using AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public class SignalRunsTracker : ISignalRunsTracker
    {
        private const string TableName = "signaltrackingtable";
        private const string PartitionKey = "signals";

        private readonly ICloudTableWrapper _trackingTable;
        private readonly ITracer _tracer;

        /// <summary>
        /// Constructor - creates the signal run tracker instance
        /// </summary>
        /// <param name="tableClient">The azure storage table client</param>
        /// <param name="tracer">Log wrapper</param>
        public SignalRunsTracker(ICloudTableClientWrapper tableClient, ITracer tracer)
        {
            _tracer = tracer;

            // set retry policy
            tableClient.DefaultRequestOptions = new TableRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 5),
                MaximumExecutionTime = TimeSpan.FromSeconds(60)
            };

            // create the cloud table instance
            _trackingTable = tableClient.GetTableReference(TableName);
            _trackingTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the IDs of the signal that needs to be executed based on configuration and their last execution times
        /// </summary>
        /// <param name="signalConfigurations">list of signal configurations</param>
        /// <returns>The signal IDs</returns>
        public async Task<IList<string>> GetSignalsToRunAsync(IEnumerable<SmartSignalConfiguration> signalConfigurations)
        {
            _tracer.TraceVerbose("getting signals to run");

            // get all last signal runs from table storage
            var signalIdToLastRun = new Dictionary<string ,TrackSignalRunEntity>();
            var allSignalRunsQuery = new TableQuery<TrackSignalRunEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<TrackSignalRunEntity> resultSegment = await _trackingTable.ExecuteQuerySegmentedAsync(allSignalRunsQuery, token);
                token = resultSegment.ContinuationToken;

                foreach (var signalRunEntity in resultSegment.Results)
                {
                    // Add to dictionary for faster lookup later on
                    signalIdToLastRun.Add(signalRunEntity.RowKey, signalRunEntity);
                }
                
            } while (token != null);

            // for each signal check if needs to be run based on its schedule and its last execution time
            var signalIdsToRun = new List<string>();
            foreach (var signalConfiguration in signalConfigurations)
            {
                bool signalWasExecutedBefore = signalIdToLastRun.TryGetValue(signalConfiguration.SignalId, out var signalLastRun);
                var nextBaseTime = signalWasExecutedBefore ? signalLastRun.LastRunTime : DateTime.MinValue;
                DateTime signalNextRun = signalConfiguration.Schedule.GetNextOccurrence(nextBaseTime);
                if (signalNextRun <= DateTime.UtcNow)
                {
                    _tracer.TraceInformation($"signal {signalConfiguration.SignalId} last ran at {signalLastRun} and is marked to run");
                    signalIdsToRun.Add(signalConfiguration.SignalId);
                }
            }

            return signalIdsToRun;
        }

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="signalId">The signal ID of the signal to update</param>
        public async Task UpdateSignalRunAsync(string signalId)
        {
            // Execute the update operation
            _tracer.TraceVerbose($"updating run for signal: {signalId}");
            var operation = TableOperation.InsertOrReplace(new TrackSignalRunEntity
            {
                PartitionKey = PartitionKey,
                RowKey = signalId,
                LastRunTime = DateTime.UtcNow
            });
            await _trackingTable.ExecuteAsync(operation);
        }
    }
}
