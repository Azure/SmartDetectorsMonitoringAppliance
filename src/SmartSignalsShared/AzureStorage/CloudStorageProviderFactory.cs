//-----------------------------------------------------------------------
// <copyright file="CloudStorageProviderFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage
{
    using System;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// An implementation of the <see cref="ICloudStorageProviderFactory"/> interface.
    /// </summary>
    public class CloudStorageProviderFactory : ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage table client for the Smart Signal storage
        /// </summary>
        /// <returns>A <see cref="ICloudTableClientWrapper"/> for the Smart Signal storage</returns>
        public ICloudTableClientWrapper GetSmartSignalStorageTableClient()
        {
            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();

            return new CloudTableClientWrapper(cloudTableClient);
        }

        /// <summary>
        /// Creates an Azure Storage container client for the signal result storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Signal result storage container</returns>
        public ICloudBlobContainerWrapper GetSmartSignalResultStorageContainer()
        {
            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudBlobClient cloudBlobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("signalresult");
            cloudBlobContainer.CreateIfNotExists();

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }

        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Signal storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Signal storage container</returns>
        public ICloudBlobContainerWrapper GetSmartSignalGlobalStorageContainer()
        {
            var cloudBlobContainerUri = new Uri(ConfigurationReader.ReadConfig("GlobalSmartSignalContainerUri", required: true));
            var cloudBlobContainer = new CloudBlobContainer(cloudBlobContainerUri);

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }
    }
}
