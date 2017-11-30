namespace Microsoft.SmartSignals.Scheduler.AzureStorage
{
    using System.Threading.Tasks;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// A wrapper of the azure cloud table
    /// </summary>
    public class CloudTableWrapper : ICloudTableWrapper
    {
        private readonly CloudTable _cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableWrapper"/> class.
        /// </summary>
        /// <param name="cloudTable">Cloud table</param>
        public CloudTableWrapper(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
        }
        
        /// <summary>
        /// Creates the table if it does not already exist.
        /// </summary>
        /// <returns>true if table was created; otherwise, false.</returns>
        public bool CreateIfNotExists()
        {
            return _cloudTable.CreateIfNotExists();
        }

        /// <summary>
        /// Initiates an asynchronous operation that executes an asynchronous table operation.
        /// </summary>
        /// <param name="operation">A <see cref="TableOperation"/> object that represents the operation to perform.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object of type <see cref="TableResult"/> that represents the asynchronous operation.</returns>
        public Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return _cloudTable.ExecuteAsync(operation);
        }

        /// <summary>
        /// Initiates an asynchronous operation to perform a segmented query on a table.
        /// </summary>
        /// <param name="query">A <see cref="TableQuery"/> representing the query to execute.</param>
        /// <param name="token">A <see cref="TableContinuationToken"/> object representing a continuation token from the server when the operation returns a partial result.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        public Task<TableQuerySegment<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token)
            where T : ITableEntity, new()
        {            
            return _cloudTable.ExecuteQuerySegmentedAsync(query, token);
        }
    }
}
