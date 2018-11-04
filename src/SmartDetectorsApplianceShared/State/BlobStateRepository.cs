//-----------------------------------------------------------------------
// <copyright file="BlobStateRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.State
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a persistent repository for storing Smart Detector related data (state) between analysis runs build on top of blob storage.
    /// </summary>
    public class BlobStateRepository : IStateRepository
    {
        private const int MaxSerializedStateLength = 1024 * 1024 * 1024;

        private readonly ICloudBlobContainerWrapper cloudBlobContainerWrapper;
        private readonly string smartDetectorId;
        private readonly string alertRuleResourceId;
        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStateRepository"/> class
        /// </summary>
        /// <param name="smartDetectorId">The ID of the Smart Detector</param>
        /// <param name="alertRuleResourceId">The resource ID of the Alert rule</param>
        /// <param name="cloudStorageProviderFactory">The cloud storage provider factory</param>
        /// <param name="tracer">The tracer</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We are converting the resource ID to lower case which is the canonized version of it")]
        public BlobStateRepository(string smartDetectorId, string alertRuleResourceId, ICloudStorageProviderFactory cloudStorageProviderFactory, IExtendedTracer tracer)
        {
            this.smartDetectorId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => smartDetectorId);
            this.alertRuleResourceId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => alertRuleResourceId).ToLowerInvariant();
            this.cloudBlobContainerWrapper = cloudStorageProviderFactory.GetSmartDetectorStateStorageContainerAsync().Result;
            this.tracer = tracer;
        }

        /// <summary>
        /// Stores <paramref name="state"/> in the repository with the specified <paramref name="key"/>.
        /// If there is already a state stored with the same key, it will be replaced by <paramref name="state"/>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will store the state in the repository as a JSON-serialized string.</typeparam>
        /// <param name="key">The state's key (case insensitive).</param>
        /// <param name="state">The state to store.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key or the state are null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state serialization fails.</exception>
        /// <exception cref="StateTooBigException">This exception is thrown if serialized state exceeds allowed length.</exception>
        /// <exception cref="FailedToSaveStateException">This exception is thrown if state saving failed due to an internal error.</exception>
        public Task StoreStateAsync<T>(string key, T state, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            key = key.ToUpperInvariant();

            this.tracer.TraceInformation($"Serializing state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            string serializedState;
            try
            {
                serializedState = JsonConvert.SerializeObject(state);
            }
            catch (Exception ex)
            {
                throw new StateSerializationException(ex);
            }

            if (serializedState.Length > MaxSerializedStateLength)
            {
                throw new StateTooBigException(serializedState.Length, MaxSerializedStateLength);
            }

            this.tracer.TraceInformation($"Compressing state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            string compressedSerializedState = CompressString(serializedState);

            BlobState blobState = new BlobState
            {
                Key = key,
                SmartDetectorId = this.smartDetectorId,
                AlertRuleResourceId = this.alertRuleResourceId,
                State = compressedSerializedState
            };

            string serializedBlobState = JsonConvert.SerializeObject(blobState);

            this.tracer.TraceInformation($"Uploading state to '{this.GenerateBlobName(key)}' for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            Task<ICloudBlob> uploadBlobTask;
            try
            {
                uploadBlobTask = this.cloudBlobContainerWrapper.UploadBlobAsync(this.GenerateBlobName(key), serializedBlobState, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new FailedToSaveStateException(ex);
            }

            this.tracer.TraceInformation($"Successfully uploaded state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");

            return uploadBlobTask;
        }

        /// <summary>
        /// Gets a Smart Detector's state that was saved with <paramref name="key"/>.
        /// If state does not exist, returns <code>default(<typeparamref name="T"/>)</code>.
        /// </summary>
        /// <typeparam name="T">The type of the state. The repository will try to JSON-deserialize the stored state to this type.</typeparam>
        /// <param name="key">The key that was used to store the state (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation, returning the requested state.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="StateSerializationException">This exception is thrown if state deserialization fails.</exception>
        /// <exception cref="FailedToLoadStateException">This exception is thrown if state retrieval failed due to an internal error.</exception>
        public async Task<T> GetStateAsync<T>(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            key = key.ToUpperInvariant();

            this.tracer.TraceInformation($"Downloading state from '{this.GenerateBlobName(key)}' for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            string serializedBlobState;
            try
            {
                serializedBlobState = await this.cloudBlobContainerWrapper.DownloadBlobContentAsync(this.GenerateBlobName(key), cancellationToken);
            }
            catch (StorageException ex) when ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                this.tracer.TraceInformation("State not found in the repository, returning empty state");
                return default(T);
            }
            catch (Exception ex)
            {
                throw new FailedToLoadStateException(ex);
            }

            this.tracer.TraceInformation($"Deserializing state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            BlobState blobState;
            try
            {
                blobState = JsonConvert.DeserializeObject<BlobState>(serializedBlobState);
            }
            catch (Exception ex)
            {
                this.tracer.ReportException(ex);
                this.tracer.TraceError("Blob state deserialization failed, trying to delete the state and returning empty state");

                // Try to delete the state
                await this.TryDeleteStateAsync(key, cancellationToken);

                return default(T);
            }

            if (!string.Equals(blobState.SmartDetectorId, this.smartDetectorId, StringComparison.InvariantCulture) ||
                !string.Equals(blobState.AlertRuleResourceId, this.alertRuleResourceId, StringComparison.InvariantCulture) ||
                !string.Equals(blobState.Key, key, StringComparison.InvariantCulture))
            {
                this.tracer.TraceError("State does not match expected Smart Detector id or key, trying to delete the state and returning empty state");

                // Try to delete the state
                await this.TryDeleteStateAsync(key, cancellationToken);

                return default(T);
            }

            if (string.IsNullOrWhiteSpace(blobState.State))
            {
                return default(T);
            }

            string compressedSerializedState = blobState.State;

            this.tracer.TraceInformation($"Decompressing state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");
            string serializedState = DecompressString(compressedSerializedState);

            T state;
            try
            {
                state = JsonConvert.DeserializeObject<T>(serializedState);
            }
            catch (Exception ex)
            {
                throw new StateSerializationException(ex);
            }

            this.tracer.TraceInformation($"Successfully retrieved state for Smart Detector {this.smartDetectorId} and rule {this.alertRuleResourceId}");

            return state;
        }

        /// <summary>
        /// Deletes the state specified by <paramref name="key"/> from the repository.
        /// </summary>
        /// <param name="key">The key of the state to delete (case insensitive).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is thrown if the key is null.</exception>
        /// <exception cref="FailedToDeleteStateException">This exception is thrown if state deletion failed due to an internal error.</exception>
        public async Task DeleteStateAsync(string key, CancellationToken cancellationToken)
        {
            Diagnostics.EnsureArgumentNotNull(() => key);

            key = key.ToUpperInvariant();

            try
            {
                await this.cloudBlobContainerWrapper.DeleteBlobIfExistsAsync(this.GenerateBlobName(key), cancellationToken);
            }
            catch (Exception ex)
            {
                throw new FailedToDeleteStateException(ex);
            }
        }

        /// <summary>
        /// Compresses a string and encodes it into base-64 formatted string
        /// </summary>
        /// <param name="stringToCompress">The string to compress</param>
        /// <returns>The compressed string</returns>
        private static string CompressString(string stringToCompress)
        {
            byte[] bytesToCompress = Encoding.UTF8.GetBytes(stringToCompress);

            using (var inputStream = new MemoryStream(bytesToCompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        inputStream.CopyTo(gzipStream);
                    }

                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Decompresses string compressed using <see cref="CompressString"/> method
        /// </summary>
        /// <param name="stringToDecompress">The string to decompress</param>
        /// <returns>The decompressed string</returns>
        private static string DecompressString(string stringToDecompress)
        {
            using (var inputStream = new MemoryStream(Convert.FromBase64String(stringToDecompress)))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(outputStream);
                        return Encoding.UTF8.GetString(outputStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Generates the name of the blob for storing the state.
        /// The name of the blob consists of alphanumeric characters of the Smart Detector id and hashes of the Smart Detector id and key.
        /// This logic insures that restrictions on blob names in storage do not cause restrictions on Smart Detector name or the key.
        /// The partial, un-hashed Smart Detector name is included for debugging purposes.
        /// </summary>
        /// <param name="key">The key (case insensitive)</param>
        /// <returns>The name of the blob</returns>
        private string GenerateBlobName(string key)
        {
            string smartDetectorIdHash = this.smartDetectorId.ToSha256Hash();
            string safeSmartDetectorId = new string(this.smartDetectorId.Where(char.IsLetterOrDigit).ToArray());

            string alertRuleResourceIdHash = this.alertRuleResourceId.ToSha256Hash();

            string keyHash = key.ToSha256Hash();

            string blobName = $"{safeSmartDetectorId}_{smartDetectorIdHash}/{alertRuleResourceIdHash}/{keyHash}";

            return blobName;
        }

        /// <summary>
        /// Deletes the state specified by <paramref name="key"/> from the repository.
        /// Does not throw under any circumstances.
        /// </summary>
        /// <param name="key">The key of the state to delete</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        private async Task TryDeleteStateAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                await this.cloudBlobContainerWrapper.DeleteBlobIfExistsAsync(this.GenerateBlobName(key), cancellationToken);
            }
            catch (Exception ex)
            {
                this.tracer.ReportException(ex);    // Do not rethrow
            }
        }

        /// <summary>
        /// Represents a Smart Detector state written to a blob
        /// </summary>
        private class BlobState
        {
            /// <summary>
            /// Gets or sets the Smart Detector ID
            /// </summary>
            public string SmartDetectorId { get; set; }

            /// <summary>
            /// Gets or sets the Alert Rule resource ID
            /// </summary>
            public string AlertRuleResourceId { get; set; }

            /// <summary>
            /// Gets or sets the Smart Detector key
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the Smart Detector serialized state
            /// </summary>
            public string State { get; set; }
        }
    }
}
