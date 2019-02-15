//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// Implementation of the <see cref="ISmartDetectorRepository"/> interface over Azure Blob Storage.
    /// The repository assumes a Smart Detector container structure of a directory for each detector and in that directory there is a package for each version of this detector.
    /// </summary>
    public class SmartDetectorRepository : ISmartDetectorRepository
    {
        private readonly IExtendedTracer tracer;
        private readonly ICloudBlobContainerWrapper containerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRepository"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="storageProviderFactory">The Azure storage provider factory</param>
        public SmartDetectorRepository(IExtendedTracer tracer, ICloudStorageProviderFactory storageProviderFactory)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.containerClient = storageProviderFactory.GetSmartDetectorGlobalStorageContainer();
        }

        /// <summary>
        /// Reads all the Smart Detectors manifests from the repository.
        /// For each Smart Detector we return the latest version's manifest.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detectors manifests</returns>
        public async Task<IList<SmartDetectorManifest>> ReadAllSmartDetectorsManifestsAsync(CancellationToken cancellationToken)
        {
            // We don't want to open the Smart Detector packages to get the manifest so we read it from the blob's metadata
            this.tracer.TraceInformation("Getting all Smart Detectors manifests from the blob metadata");
            try
            {
                var allSmartDetectorsManifests = new List<SmartDetectorManifest>();
                IEnumerable<CloudBlob> blobs = (await this.containerClient.ListBlobsAsync(string.Empty, true, BlobListingDetails.Metadata, cancellationToken)).Cast<CloudBlob>().Where(blob => blob.Metadata.ContainsKey("id"));

                ILookup<string, CloudBlob> smartDeterctorIdToAllVersionsLookup = blobs.ToLookup(blob => blob.Metadata["id"], blob => blob);
                foreach (IGrouping<string, CloudBlob> smartDeterctorVersionsGroup in smartDeterctorIdToAllVersionsLookup)
                {
                    string smartDeterctorId = smartDeterctorVersionsGroup.Key;
                    if (string.IsNullOrWhiteSpace(smartDeterctorId))
                    {
                        // blob is not a Smart Detector
                        continue;
                    }

                    // Get the latest version blob of the Smart Detector
                    CloudBlob latestVersionSmartDetectorBlob = GetLatestVersionSmartDetectorBlob(smartDeterctorIdToAllVersionsLookup[smartDeterctorId]);

                    if (latestVersionSmartDetectorBlob != null)
                    {
                        // Generate the manifest from the blob's metadata
                        allSmartDetectorsManifests.Add(GenerateSmartDetectorManifest(latestVersionSmartDetectorBlob));
                    }
                }

                return allSmartDetectorsManifests;
            }
            catch (StorageException e)
            {
                throw new SmartDetectorRepositoryException("Failed to get all Smart Detector manifests from storage", e);
            }
        }

        /// <summary>
        /// Reads a Smart Detector's package from the repository
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector's ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detector package</returns>
        public async Task<SmartDetectorPackage> ReadSmartDetectorPackageAsync(string smartDetectorId, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Getting Smart Detector {smartDetectorId} package");
            try
            {
                CloudBlob latestVersionSmartDetectorBlob = await this.GetLatestSmartDetectorBlobVersionAsync(smartDetectorId, cancellationToken);
                this.tracer.TraceInformation($"Last version Smart Detector BLOB is {latestVersionSmartDetectorBlob.Name}");

                using (var blobMemoryStream = new MemoryStream())
                {
                    // Download the blob to a stream and generate the Smart Detector package from it
                    await latestVersionSmartDetectorBlob.DownloadToStreamAsync(blobMemoryStream, cancellationToken);
                    return SmartDetectorPackage.CreateFromStream(blobMemoryStream);
                }
            }
            catch (StorageException e)
            {
                throw new SmartDetectorRepositoryException("Failed to get Smart Detector package from storage", e);
            }
        }

        /// <summary>
        /// Reads a Smart Detector's manifest from the repository
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector's ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning the Smart Detector manifest</returns>
        public async Task<SmartDetectorManifest> ReadSmartDetectorManifestAsync(string smartDetectorId, CancellationToken cancellationToken)
        {
            // We don't want to open the Smart Detector packages to get the manifest so we read it from the blob's metadata
            this.tracer.TraceInformation($"Getting Smart Detector {smartDetectorId} manifest");
            try
            {
                CloudBlob latestVersionSmartDetectorBlob = await this.GetLatestSmartDetectorBlobVersionAsync(smartDetectorId, cancellationToken);
                this.tracer.TraceInformation($"Last version Smart Detector BLOB is {latestVersionSmartDetectorBlob.Name}");

                return GenerateSmartDetectorManifest(latestVersionSmartDetectorBlob);
            }
            catch (StorageException e)
            {
                throw new SmartDetectorRepositoryException("Failed to get Smart Detector manifest from storage", e);
            }
        }

        /// <summary>
        /// Gets the latest version blob from the blob list.
        /// </summary>
        /// <param name="blobs">A collection of blobs</param>
        /// <returns>the latest version Smart Detector blob</returns>
        private static CloudBlob GetLatestVersionSmartDetectorBlob(IEnumerable<CloudBlob> blobs)
        {
            var latestVersionBlob =
                blobs.Where(blob => blob.Metadata.ContainsKey("version"))
                    .Aggregate((blob1, blob2) =>
                    {
                        if (!Version.TryParse(blob1.Metadata["version"], out Version smartDetectorVersion1))
                        {
                            return blob2;
                        }

                        if (!Version.TryParse(blob2.Metadata["version"], out Version smartDetectorVersion2))
                        {
                            return blob1;
                        }

                        return smartDetectorVersion1 > smartDetectorVersion2 ? blob1 : blob2;
                    });

            if (Version.TryParse(latestVersionBlob.Metadata["version"], out var _))
            {
                return latestVersionBlob;
            }

            // no valid version blob was found
            return null;
        }

        /// <summary>
        /// Generates a <see cref="SmartDetectorManifest"/> from the blob's metadata
        /// </summary>
        /// <param name="smartDetectorBlob">The smart detector cloud blob</param>
        /// <returns>A <see cref="SmartDetectorManifest"/> representing the Smart Detector's manifest</returns>
        private static SmartDetectorManifest GenerateSmartDetectorManifest(CloudBlob smartDetectorBlob)
        {
            IDictionary<string, string> smartDetectorMetadata = smartDetectorBlob.Metadata;
            var supportedResourceTypes = JArray.Parse(smartDetectorMetadata["supportedResourceTypes"])
                .Select(jtoken => (ResourceType)Enum.Parse(typeof(ResourceType), jtoken.ToString(), true))
                .ToList();

            var supportedCadencesInMinutes = JArray.Parse(smartDetectorMetadata["supportedCadencesInMinutes"])
                .Select(jToken => int.Parse(jToken.ToString(), CultureInfo.InvariantCulture))
                .ToList();

            List<string> imagePaths = null;
            bool imagePathsDefined = smartDetectorMetadata.TryGetValue("imagePaths", out string imagePathsString);
            if (imagePathsDefined)
            {
                // since we use the global storage for now then we can't create SAS since we don't have the private key in the monitoring appliance.
                // if the detector is in a non-public container in the future then we will need to assign a SAS to the image URI
                imagePaths = JArray.Parse(imagePathsString)
                    .Select(jToken => $"{smartDetectorBlob.Parent.Uri.AbsoluteUri}{jToken.ToString().Replace("\\", "/")}")
                    .ToList();
            }

            List<DetectorParameterDefinition> parametersDefinitions = null;
            if (smartDetectorMetadata.ContainsKey("parametersDefinitions"))
            {
                parametersDefinitions =
                    JsonConvert.DeserializeObject<List<DetectorParameterDefinition>>(smartDetectorMetadata["parametersDefinitions"]);
            }

            return new SmartDetectorManifest(
                 smartDetectorMetadata["id"],
                 smartDetectorMetadata["name"],
                 smartDetectorMetadata["description"],
                 Version.Parse(smartDetectorMetadata["version"]),
                 smartDetectorMetadata["assemblyName"],
                 smartDetectorMetadata["className"],
                 supportedResourceTypes,
                 supportedCadencesInMinutes,
                 imagePaths,
                 parametersDefinitions);
        }

        /// <summary>
        /// Gets the latest Smart Detector blob version
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector ID</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="CloudBlob"/> representing the Smart Detector package blob</returns>
        private async Task<CloudBlob> GetLatestSmartDetectorBlobVersionAsync(string smartDetectorId, CancellationToken cancellationToken)
        {
            List<CloudBlob> blobs = (await this.containerClient.ListBlobsAsync($"{smartDetectorId}/", true, BlobListingDetails.Metadata, cancellationToken)).Cast<CloudBlob>().Where(blob => blob.Metadata.ContainsKey("id")).ToList();
            if (!blobs.Any())
            {
                throw new SmartDetectorNotFoundException($"No Smart Detector package exists for detector {smartDetectorId}");
            }

            CloudBlob latestVersionSmartDetectorBlob = GetLatestVersionSmartDetectorBlob(blobs);
            if (latestVersionSmartDetectorBlob == null)
            {
                throw new SmartDetectorNotFoundException($"No Smart Detector package with a valid version exists for detector {smartDetectorId}");
            }

            return latestVersionSmartDetectorBlob;
        }
    }
}