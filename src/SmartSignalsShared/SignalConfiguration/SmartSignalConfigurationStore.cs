namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;
    using NCrontab;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalConfigurationStore"/> using Azure table.
    /// </summary>
    public class SmartSignalConfigurationStore : ISmartSignalConfigurationStore
    {
        private const string TableName = "signalconfiguration";
        private const string PartitionKey = "configurations";

        private readonly ICloudTableWrapper _configurationTable;
        private readonly ITracer _tracer;

        /// <summary>
        /// Constructor - creates the smart signal configuration store
        /// </summary>
        /// <param name="tableClient">The azure storage table client</param>
        /// <param name="tracer">Log wrapper</param>
        public SmartSignalConfigurationStore(ICloudTableClientWrapper tableClient, ITracer tracer)
        {
            _tracer = tracer;

            // set retry policy
            tableClient.DefaultRequestOptions = new TableRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 5),
                MaximumExecutionTime = TimeSpan.FromSeconds(60)
            };

            // create the cloud table instance
            _configurationTable = tableClient.GetTableReference(TableName);
            _configurationTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets all the signal configurations from the store.
        /// </summary>
        /// <returns>A <see cref="IList{SmartSignalConfiguration}"/> containing all the signal configurations in the store.</returns>
        public async Task<IList<SmartSignalConfiguration>> GetAllSmartSignalConfigurationsAsync()
        {
            _tracer.TraceInformation("Getting all smart signal configurations");
            var signalConfigurationEntities = await _configurationTable.ReadPartitionAsync<SmartConfigurationEntity>(PartitionKey);
            _tracer.TraceInformation($"Found {signalConfigurationEntities.Count} signal configurations");

            _tracer.TraceVerbose($"Found configurations for signals: {string.Join(", ", signalConfigurationEntities.Select(e => e.RowKey))}");

            return signalConfigurationEntities.Select(entity => new SmartSignalConfiguration
            {
                SignalId = entity.RowKey,
                ResourceType = entity.ResourceType,
                Schedule = CrontabSchedule.Parse(entity.CrontabSchedule)
            }).ToList();
        }

        /// <summary>
        /// Adds or updates a signal configuration in the store.
        /// </summary>
        /// <param name="signalConfiguration">The signal configuration to add to the store.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        public async Task AddOrReplaceSmartSignalConfigurationAsync(SmartSignalConfiguration signalConfiguration)
        {
            // Execute the update operation
            _tracer.TraceInformation($"updating signal configuration for: {signalConfiguration.SignalId}");
            var operation = TableOperation.InsertOrReplace(new SmartConfigurationEntity
            {
                PartitionKey = PartitionKey,
                RowKey = signalConfiguration.SignalId,
                ResourceType = signalConfiguration.ResourceType,
                CrontabSchedule = signalConfiguration.Schedule.ToString()
            });

            await _configurationTable.ExecuteAsync(operation);

            _tracer.TraceInformation($"updated signal configuration for: {signalConfiguration.SignalId}");
        }
    }
}
