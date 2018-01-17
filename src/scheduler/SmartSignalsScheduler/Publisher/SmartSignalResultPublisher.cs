//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for publishing Smart Signal results.
    /// </summary>
    public class SmartSignalResultPublisher : ISmartSignalResultPublisher
    {
        private const string ResultEventName = "SmartSignalResult";

        private readonly ITracer tracer;
        private readonly ICloudBlobContainerWrapper containerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultPublisher"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        /// <param name="storageProviderFactory">The Azure storage provider factory.</param>
        public SmartSignalResultPublisher(ITracer tracer, ICloudStorageProviderFactory storageProviderFactory)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.containerClient = storageProviderFactory.GetSmartSignalResultStorageContainer();
        }

        /// <summary>
        /// Publish Smart Signal result items as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="smartSignalResultItems">The Smart Signal result items to publish</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        public async Task PublishSignalResultItemsAsync(string signalId, IList<SmartSignalResultItemPresentation> smartSignalResultItems)
        {
            if (smartSignalResultItems == null || !smartSignalResultItems.Any())
            {
                this.tracer.TraceInformation($"no result items to publish for signal {signalId}");
                return;
            }

            try
            {
                var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
                foreach (var resultItem in smartSignalResultItems)
                {
                    var blobName = $"{signalId}/{todayString}/{resultItem.Id}";
                    var resultItemString = JsonConvert.SerializeObject(resultItem);
                    ICloudBlob blob = await this.containerClient.UploadBlobAsync(blobName, resultItemString);

                    var eventProperties = new Dictionary<string, string>
                    {
                        { "SignalId", signalId },
                        { "ResultItemBlobUri", blob.Uri.AbsoluteUri }
                    };

                    this.tracer.TrackEvent(ResultEventName, eventProperties);
                }
            }
            catch (StorageException e)
            {
                this.tracer.TraceError($"Failed to publish signal results to storage for {signalId} with exception: {e}");
                throw new SignalResultPublishException($"Failed to publish signal results to storage for {signalId}", e);
            }

            this.tracer.TraceInformation($"{smartSignalResultItems.Count} Smart Signal result items for signal {signalId} were published to the results store");
        }
    }
}
