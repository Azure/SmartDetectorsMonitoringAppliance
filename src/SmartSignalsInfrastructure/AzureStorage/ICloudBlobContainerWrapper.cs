//-----------------------------------------------------------------------
// <copyright file="ICloudBlobContainerWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AzureStorage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// An interface for an Azure blob container wrapper
    /// </summary>
    public interface ICloudBlobContainerWrapper
    {
        /// <summary>
        /// Returns a list of the blobs in the container.
        /// </summary>
        /// <param name="prefix">A string containing the blob name prefix.</param>
        /// <param name="useFlatBlobListing">A boolean value that specifies whether to list blobs in a flat listing, or whether to list blobs hierarchically, by virtual directory.</param>
        /// <param name="blobListingDetails">A BlobListingDetails enumeration describing which items to include in the listing.</param>
        /// <returns>A list of objects that implement <see cref="IListBlobItem"/></returns>
        Task<IList<IListBlobItem>> ListBlobsAsync(string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails);
    }
}
