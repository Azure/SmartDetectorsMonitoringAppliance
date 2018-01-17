//-----------------------------------------------------------------------
// <copyright file="ICloudTableWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AzureStorage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// An interface for a Azure Storage table wrapper
    /// </summary>
    public interface ICloudTableWrapper
    {
        /// <summary>
        /// Creates the table if it does not already exist.
        /// </summary>
        /// <returns>true if table was created; otherwise, false.</returns>
        bool CreateIfNotExists();

        /// <summary>
        /// Initiates an asynchronous operation that executes an asynchronous table operation.
        /// </summary>
        /// <param name="operation">A <see cref="TableOperation"/> object that represents the operation to perform.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object of type <see cref="TableResult"/> that represents the asynchronous operation.</returns>
        Task<TableResult> ExecuteAsync(TableOperation operation);

        /// <summary>
        /// Retrieves all entities with the given partition key
        /// </summary>
        /// <typeparam name="T">The type of the entity to return.</typeparam>
        /// <param name="partitionKey">A string containing the partition key</param>
        /// <returns>A <see cref="IList{T}"/> containing all entities of the given partition key</returns>
        Task<IList<T>> ReadPartitionAsync<T>(string partitionKey) where T : ITableEntity, new();
    }
}
