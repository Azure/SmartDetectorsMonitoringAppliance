namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage
{
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// An interface for a auzre table client wrapper
    /// </summary>
    public interface ICloudTableClientWrapper
    {
        /// <summary>
        /// Gets or sets the default request options for requests made via the Table service client.
        /// </summary>
        TableRequestOptions DefaultRequestOptions { get; set; }

        /// <summary>
        /// Gets a reference to the specified table.
        /// </summary>
        /// <param name="tableName">A string containing the name of the table.</param>
        /// <returns>A <see cref="CloudTable"/> object.</returns>
        ICloudTableWrapper GetTableReference(string tableName);
    }
}
