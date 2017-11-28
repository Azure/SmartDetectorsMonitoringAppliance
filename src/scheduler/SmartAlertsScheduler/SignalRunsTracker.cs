namespace Microsoft.SmartSignals.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.SmartSignals.Scheduler;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using SmartAlerts.Shared;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public class SignalRunsTracker : ISignalRunsTracker
    {
        // Hold current time of day as a field for testing purposes.
        internal TimeSpan CurrentTimeOfDayUtc;

        public const string TableName = "jobtrackingtable";
        private CloudTable _discoveryTrackingTable;
        private readonly Policy _policy;
        private readonly DateTime _currentPositionDate;
        private readonly string _jobName;
        private readonly string _connectionString;
        private readonly double _hourOfFirstRetry;
        private readonly double _hourOfLastRetry;

        /// <summary>
        /// Constructor - creates the table for tracking
        /// </summary>
        /// <param name="storageConnectionString">Connecting string for the storage that stores the tracking table</param>
        /// <param name="jobName">The job name</param>
        /// <param name="position">The position for job to run on</param>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="hourOfFirstRetry">The delay in hours from midnight of the tracked job</param>
        /// <param name="hourOfLastRetry">The exhausted time in UTC for the job - if passed, the job should not retry any more.</param>
        public SignalRunsTracker(string storageConnectionString, string jobName, DateTime position, double hourOfFirstRetry, double hourOfLastRetry)
        {
            _currentPositionDate = position;
            _jobName = jobName;
            _tracer = tracer;
            _connectionString = storageConnectionString;
            _hourOfLastRetry = hourOfLastRetry;
            _hourOfFirstRetry = hourOfFirstRetry;
            CurrentTimeOfDayUtc = DateTime.UtcNow.TimeOfDay;
            _policy = Policy.Handle<Exception>(ex => true)
            .WaitAndRetry(5, i => TimeSpan.FromSeconds(Math.Pow(2, i)),
                (exception, span, context) =>
                {
                    _tracer.TraceVerbose($"Failed executing on {exception}, retry {Math.Log(span.Seconds, 2)} out of 5");
                });
        }

        /// <summary>
        /// Checks in the azure table whether a run is needed - if there wasn't successful run already for this position
        /// </summary>
        public bool ShouldRun()
        {
            _tracer.TraceInformation($"checking if run is needed. jobName: {_jobName}. position: {_currentPositionDate}");
            this.InitClient();

            TableResult retrievedResult = null;

            // Create a retrieve operation that takes a customer entity.
            _policy.Execute(() =>
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<TrackJobEntity>(_jobName, _jobName);

                // Execute the retrieve operation.
                retrievedResult = _discoveryTrackingTable.Execute(retrieveOperation);
            });

            return this.ShouldRun(retrievedResult?.Result);
        }

        /// <summary>
        /// Updates the azure table stores the last run position with this run  
        /// </summary>
        public void UpdateRun()
        {
            this.InitClient();

            _policy.Execute(() =>
            {
                // Create an insertOrReplace operation that takes a customer entity.
                var insertOperation = TableOperation.InsertOrReplace(new TrackJobEntity
                {
                    RowKey = _jobName,
                    PartitionKey = _jobName,
                    LastRun = _currentPositionDate
                });

                // Execute the insert operation.
                _tracer.TraceVerbose($"updating run. jobName: {_jobName}. position: {_currentPositionDate}");
                _discoveryTrackingTable.Execute(insertOperation);
                _tracer.TraceVerbose($"updated run jobName: {_jobName}. position: {_currentPositionDate}");
            });
        }

        internal bool ShouldRun(object lastRunResult)
        {
            if (lastRunResult != null)
            {
                DateTime lastRun = ((TrackJobEntity)lastRunResult).LastRun;
                if (_currentPositionDate == lastRun)
                {
                    // Already ran on this day
                    _tracer.TraceInformation("Already ran today. Should not run.");
                    return false;
                }
            }

            // Didn't finish a successful run but discover delay was not reached yet
            if (CurrentTimeOfDayUtc.TotalHours < _hourOfFirstRetry)
            {
                _tracer.TraceInformation("should run but _hourOfFirstRetry not reached. " +
                                         $"current time UTC: {CurrentTimeOfDayUtc} _JobDelay UTC: {_hourOfFirstRetry}");
                return false;
            }

            // Didn't finish a successful run but exhausted time was reached - we add 1 in order to keep
            // the semantics of "hour of last retry" (e.g. if _hourOfLastRetry==4 then we'll retry anywhere inside the
            // 4am hour)
            if (CurrentTimeOfDayUtc.TotalHours >= _hourOfLastRetry + 1)
            {
                _tracer.TraceInformation("should run but _exhaustedRetriesTimeUtc reached. " +
                                         $"current time UTC: {CurrentTimeOfDayUtc} _exhausteTimeUtc: {_hourOfLastRetry}");
                return false;
            }

            // Not finished a successful run - return true
            _tracer.TraceInformation("Should run again");
            return true;
        }

        private void InitClient()
        {
            // Get the table - create if not exist
            _policy.Execute(() =>
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                _discoveryTrackingTable = tableClient.GetTableReference(TableName);
                _discoveryTrackingTable.CreateIfNotExists();
            });
        }

        public Task<IEnumerable<string>> GetSignalsToRunAsync(IEnumerable<SignalConfiguration> signalConfigurations)
        {
            signalConfigurations.First().Schedule.GetNextOccurrence(DateTime.UtcNow)
        }

        public Task UpdateSignalRun(string signalId)
        {
            throw new NotImplementedException();
        }
    }
}
