namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage
{

    /// <summary>
    /// An interface for a auzre table client wrapper
    /// </summary>
    public interface ICloudTableClientWrapper
    {
        /// <summary>
        /// Gets a reference to the specified table.
        /// </summary>
        /// <param name="tableName">A string containing the name of the table.</param>
        /// <returns>A <see cref="ICloudTableWrapper"/> object.</returns>
        ICloudTableWrapper GetTableReference(string tableName);
    }
}
