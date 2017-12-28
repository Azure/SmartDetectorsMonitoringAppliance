//-----------------------------------------------------------------------
// <copyright file="CloudBlobContainerWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// A wrapper of the Azure cloud blob container
    /// </summary>
    public class CloudBlobContainerWrapper : ICloudBlobContainerWrapper
    {
        private readonly CloudBlobContainer cloudBlobContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBlobContainerWrapper"/> class.
        /// </summary>
        /// <param name="cloudBlobContainer">Cloud blob container</param>
        public CloudBlobContainerWrapper(CloudBlobContainer cloudBlobContainer)
        {
            this.cloudBlobContainer = cloudBlobContainer;
        }

        /// <summary>
        /// Returns a list of the blobs in the container.
        /// </summary>
        /// <param name="prefix">A string containing the blob name prefix.</param>
        /// <param name="useFlatBlobListing">A boolean value that specifies whether to list blobs in a flat listing, or whether to list blobs hierarchically, by virtual directory.</param>
        /// <param name="blobListingDetails">A BlobListingDetails enumeration describing which items to include in the listing.</param>
        /// <returns>A list of objects that implement <see cref="IListBlobItem"/></returns>
        public async Task<IList<IListBlobItem>> ListBlobsAsync(string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails)
        {
            var blobs = new List<IListBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                var resultSegment = await this.cloudBlobContainer.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, null, token, null, null);
                token = resultSegment.ContinuationToken;
                blobs.AddRange(resultSegment.Results);
            }
            while (token != null);

            return blobs;
        }
    }
}
