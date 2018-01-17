//-----------------------------------------------------------------------
// <copyright file="CloudTableClientWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AzureStorage
{
    using System;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A wrapper of the azure cloud table client
    /// </summary>
    public class CloudTableClientWrapper : ICloudTableClientWrapper
    {
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableClientWrapper"/> class.
        /// </summary>
        /// <param name="cloudTableClient">Cloud table client</param>
        public CloudTableClientWrapper(CloudTableClient cloudTableClient)
        {
            this.cloudTableClient = cloudTableClient;
            
            // set retry policy
            this.cloudTableClient.DefaultRequestOptions = new TableRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 5),
                MaximumExecutionTime = TimeSpan.FromSeconds(60)
            };
        }

        /// <summary>
        /// Gets a reference to the specified table.
        /// </summary>
        /// <param name="tableName">A string containing the name of the table.</param>
        /// <returns>A <see cref="CloudTable"/> object.</returns>
        public ICloudTableWrapper GetTableReference(string tableName)
        {
            return new CloudTableWrapper(this.cloudTableClient.GetTableReference(tableName));
        }
    }
}
