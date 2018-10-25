//-----------------------------------------------------------------------
// <copyright file="CloudStorageProviderFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// An implementation of the <see cref="ICloudStorageProviderFactory"/> interface.
    /// </summary>
    public class CloudStorageProviderFactory : ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Detector storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector storage container</returns>
        public ICloudBlobContainerWrapper GetSmartDetectorGlobalStorageContainer()
        {
            var cloudBlobContainerUri = new Uri(ConfigurationReader.ReadConfig("GlobalSmartDetectorContainerUri", required: true));
            var cloudBlobContainer = new CloudBlobContainer(cloudBlobContainerUri);

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }

        /// <summary>
        /// Creates an Azure Storage container client for the Smart Detector state storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector state storage container</returns>
        public async Task<ICloudBlobContainerWrapper> GetSmartDetectorStateStorageContainerAsync()
        {
            return await GetLocalStorageContainerAsync("state");
        }

        /// <summary>
        /// Creates an Azure Storage container client for a local Monitoring Appliance container
        /// </summary>
        /// <param name="containerName">The name of the container to create.</param>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the container</returns>
        private static async Task<ICloudBlobContainerWrapper> GetLocalStorageContainerAsync(string containerName)
        {
            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudBlobClient cloudBlobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }
    }
}
