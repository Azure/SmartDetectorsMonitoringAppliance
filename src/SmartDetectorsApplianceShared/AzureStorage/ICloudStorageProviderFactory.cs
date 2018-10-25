//-----------------------------------------------------------------------
// <copyright file="ICloudStorageProviderFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for exposing a factory that creates Azure Storage clients
    /// </summary>
    public interface ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Detector storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector storage container</returns>
        ICloudBlobContainerWrapper GetSmartDetectorGlobalStorageContainer();

        /// <summary>
        /// Creates an Azure Storage container client for the Smart Detector state container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector state container</returns>
        Task<ICloudBlobContainerWrapper> GetSmartDetectorStateStorageContainerAsync();
    }
}
