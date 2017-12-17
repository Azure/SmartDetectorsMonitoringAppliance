namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using NCrontab;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalConfigurationStore"/> using Azure table.
    /// </summary>
    public class SmartSignalConfigurationStore : ISmartSignalConfigurationStore
    {
        private const string TableName = "signalconfiguration";
        private const string PartitionKey = "configurations";

        private readonly ICloudTableWrapper configurationTable;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the<see cref="SmartSignalConfigurationStore"/> class.
        /// </summary>
        /// <param name="tableClient">The azure storage table client</param>
        /// <param name="tracer">Log wrapper</param>
        public SmartSignalConfigurationStore(ICloudTableClientWrapper tableClient, ITracer tracer)
        {
            this.tracer = tracer;

            // create the cloud table instance
            this.configurationTable = tableClient.GetTableReference(TableName);
            this.configurationTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets all the signal configurations from the store.
        /// </summary>
        /// <returns>A <see cref="IList{SmartSignalConfiguration}"/> containing all the signal configurations in the store.</returns>
        public async Task<IList<SmartSignalConfiguration>> GetAllSmartSignalConfigurationsAsync()
        {
            try
            {
                this.tracer.TraceInformation("Getting all smart signal configurations");
                var signalConfigurationEntities = await this.configurationTable.ReadPartitionAsync<SmartConfigurationEntity>(PartitionKey);
                this.tracer.TraceInformation($"Found {signalConfigurationEntities.Count} signal configurations");

                this.tracer.TraceVerbose($"Found configurations for signals: {string.Join(", ", signalConfigurationEntities.Select(e => e.RowKey))}");

                return signalConfigurationEntities.Select(entity => new SmartSignalConfiguration
                {
                    SignalId = entity.RowKey,
                    ResourceType = entity.ResourceType,
                    Schedule = CrontabSchedule.Parse(entity.CrontabSchedule)
                }).ToList();
            }
            catch (StorageException e)
            {
                throw new SmartSignalConfigurationStoreException("Failed to get smart signals", e);
            }
        }

        /// <summary>
        /// Adds or updates a signal configuration in the store.
        /// </summary>
        /// <param name="signalConfiguration">The signal configuration to add to the store.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        public async Task AddOrReplaceSmartSignalConfigurationAsync(SmartSignalConfiguration signalConfiguration)
        {
            try
            {
                // Execute the update operation
                this.tracer.TraceInformation($"updating signal configuration for: {signalConfiguration.SignalId}");
                var operation = TableOperation.InsertOrReplace(new SmartConfigurationEntity
                {
                    PartitionKey = PartitionKey,
                    RowKey = signalConfiguration.SignalId,
                    ResourceType = signalConfiguration.ResourceType,
                    CrontabSchedule = signalConfiguration.Schedule.ToString()
                });

                await this.configurationTable.ExecuteAsync(operation);

                this.tracer.TraceInformation($"updated signal configuration for: {signalConfiguration.SignalId}");
            }
            catch (StorageException e)
            {
                throw new SmartSignalConfigurationStoreException("Failed to add/replace smart signal", e);
            }
        }
    }
}
