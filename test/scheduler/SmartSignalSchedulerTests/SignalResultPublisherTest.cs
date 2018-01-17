//-----------------------------------------------------------------------
// <copyright file="SignalResultPublisherTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.SignalResultPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;

    [TestClass]
    public class SignalResultPublisherTest
    {
        private Mock<ITracer> tracerMock;
        private Mock<ICloudBlobContainerWrapper> containerMock;

        private SmartSignalResultPublisher publisher;

        [TestInitialize]
        public void Setup()
        {
            this.tracerMock = new Mock<ITracer>();
            this.containerMock = new Mock<ICloudBlobContainerWrapper>();

            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartSignalResultStorageContainer()).Returns(this.containerMock.Object);

            this.publisher = new SmartSignalResultPublisher(this.tracerMock.Object, storageProviderFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenNoResultsToPublishThenResultStoreIsNotCalled()
        {
            await this.publisher.PublishSignalResultItemsAsync("signalId", new List<SmartSignalResultItemPresentation>());

            this.containerMock.Verify(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            this.tracerMock.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(SignalResultPublishException))]
        public async Task WhenStorageExceptionIsThrownThenPublisherExceptionIsThrown()
        {
            // Setup mock to throw storage exception when correct blob name is specified
            var signalId = "signalId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName = $"{signalId}/{todayString}/id1";
            this.containerMock.Setup(m => m.UploadBlobAsync(blobName, It.IsAny<string>())).Throws(new StorageException());

            var resultItems = new List<SmartSignalResultItemPresentation>
            {
                new SmartSignalResultItemPresentation("id1", "title1", null, "resource1", null, signalId, signalId, DateTime.UtcNow, 10, null, null),
                new SmartSignalResultItemPresentation("id2", "title2", null, "resource2", null, signalId, signalId, DateTime.UtcNow, 10, null, null)
            };
            await this.publisher.PublishSignalResultItemsAsync("signalId", resultItems);
        }

        [TestMethod]
        public async Task WhenPublishingResultsThenResultStoreIsCalledAccordingly()
        {
            // Setup mock to return blob with URI when correct blob name is specified
            var signalId = "signalId";
            var todayString = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var blobName1 = $"{signalId}/{todayString}/id1";
            var blobName2 = $"{signalId}/{todayString}/id2";

            var blobMock1 = new Mock<ICloudBlob>();
            var blobUri1 = new Uri($"https://storage.blob.core.windows.net/result/{blobName1}");
            blobMock1.Setup(m => m.Uri).Returns(blobUri1);

            var blobMock2 = new Mock<ICloudBlob>();
            var blobUri2 = new Uri($"https://storage.blob.core.windows.net/result/{blobName2}");
            blobMock2.Setup(m => m.Uri).Returns(blobUri2);

            this.containerMock.Setup(m => m.UploadBlobAsync(blobName1, It.IsAny<string>())).ReturnsAsync(blobMock1.Object);
            this.containerMock.Setup(m => m.UploadBlobAsync(blobName2, It.IsAny<string>())).ReturnsAsync(blobMock2.Object);

            var resultItems = new List<SmartSignalResultItemPresentation>
            {
                new SmartSignalResultItemPresentation("id1", "title1", null, "resource1", null, signalId, signalId, DateTime.UtcNow, 10, null, null),
                new SmartSignalResultItemPresentation("id2", "title2", null, "resource2", null, signalId, signalId, DateTime.UtcNow, 10, null, null)
            };
            await this.publisher.PublishSignalResultItemsAsync(signalId, resultItems);

            this.tracerMock.Verify(m => m.TrackEvent("SmartSignalResult", It.Is<IDictionary<string, string>>(properties => properties["SignalId"] == signalId && properties["ResultItemBlobUri"] == blobUri1.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
            this.tracerMock.Verify(m => m.TrackEvent("SmartSignalResult", It.Is<IDictionary<string, string>>(properties => properties["SignalId"] == signalId && properties["ResultItemBlobUri"] == blobUri2.AbsoluteUri), It.IsAny<IDictionary<string, double>>()), Times.Once);
        }
    }
}
