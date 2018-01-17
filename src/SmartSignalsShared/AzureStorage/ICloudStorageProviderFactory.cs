//-----------------------------------------------------------------------
// <copyright file="ICloudStorageProviderFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage
{
    /// <summary>
    /// An interface for exposing a factory that creates Azure Storage clients
    /// </summary>
    public interface ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage table client for the Smart Signal storage
        /// </summary>
        /// <returns>A <see cref="ICloudTableClientWrapper"/> for the Smart Signal storage</returns>
        ICloudTableClientWrapper GetSmartSignalStorageTableClient();

        /// <summary>
        /// Creates an Azure Storage container client for the signal result storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Signal result storage container</returns>
        ICloudBlobContainerWrapper GetSmartSignalResultStorageContainer();

        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Signal storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Signal storage container</returns>
        ICloudBlobContainerWrapper GetSmartSignalGlobalStorageContainer();
    }
}
