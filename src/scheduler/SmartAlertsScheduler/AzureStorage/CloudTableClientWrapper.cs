namespace Microsoft.SmartSignals.Scheduler.AzureStorage
{
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// A wrapper of the azure cloud table client
    /// </summary>
    public class CloudTableClientWrapper : ICloudTableClientWrapper
    {
        private readonly CloudTableClient _cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableClientWrapper"/> class.
        /// </summary>
        /// <param name="cloudTableClient">Cloud table client</param>
        public CloudTableClientWrapper(CloudTableClient cloudTableClient)
        {
            _cloudTableClient = cloudTableClient;
        }

        /// <summary>
        /// Gets or sets the default request options for requests made via the Table service client.
        /// </summary>
        public TableRequestOptions DefaultRequestOptions { get; set; }

        /// <summary>
        /// Gets a reference to the specified table.
        /// </summary>
        /// <param name="tableName">A string containing the name of the table.</param>
        /// <returns>A <see cref="CloudTable"/> object.</returns>
        public ICloudTableWrapper GetTableReference(string tableName)
        {
            return new CloudTableWrapper(_cloudTableClient.GetTableReference(tableName));
        }
    }
}
