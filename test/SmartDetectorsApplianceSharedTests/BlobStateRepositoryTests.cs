//-----------------------------------------------------------------------
// <copyright file="BlobStateRepositoryTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsApplianceSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;

    [TestClass]
    public class BlobStateRepositoryTests
    {
        [TestMethod]
        public async Task WhenExecutingFullStateActionsThenFlowCompletesSuccesfully()
        {
            Dictionary<string, string> repository = new Dictionary<string, string>();
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();
            cloudBlobContainerWrapperMock
                .Setup(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                .Returns<string, string, CancellationToken>((blobName, blobContent, token) =>
                {
                    repository[blobName] = blobContent;
                    return Task.FromResult<ICloudBlob>(null);
                });
            cloudBlobContainerWrapperMock
                .Setup(m => m.DownloadBlobContentAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns<string, CancellationToken>((blobName, token) =>
                {
                    if (repository.ContainsKey(blobName))
                    {
                        return Task.FromResult<string>(repository[blobName]);
                    }
                    else
                    {
                        StorageException ex = new StorageException(new RequestResult(), string.Empty, null);
                        ex.RequestInformation.HttpStatusCode = (int)HttpStatusCode.NotFound;
                        throw ex;
                    }
                });
            cloudBlobContainerWrapperMock
                .Setup(m => m.DeleteBlobIfExistsAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns<string, CancellationToken>((blobName, token) =>
                {
                    repository.Remove(blobName);
                    return Task.CompletedTask;
                });

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            await TestFullFlow(blobStateRepository);
        }

        [TestMethod]
        [Ignore]
        public async Task WhenExecutingFullStateActionsThenFlowCompletesSuccesfullyWithRealStorage()
        {
            var storageConnectionString = "****";
            CloudBlobClient cloudBlobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("tesdetectorstatecontainer");
            await cloudBlobContainer.CreateIfNotExistsAsync();
            CloudBlobContainerWrapper cloudBlobContainerWrapper = new CloudBlobContainerWrapper(cloudBlobContainer);

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapper);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            await TestFullFlow(blobStateRepository);

            // delete all remaining blobs
            foreach (var blob in cloudBlobContainer.ListBlobs(useFlatBlobListing: true))
            {
                var blockblob = blob as CloudBlockBlob;
                await blockblob?.DeleteIfExistsAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToLoadStateException))]
        public async Task WhenStorageIsBrokenThenAnAppropriateExceptionIsThrownOnGetState()
        {
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();
            cloudBlobContainerWrapperMock
                .Setup(m => m.DownloadBlobContentAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns(() => throw new StorageException(new RequestResult(), string.Empty, null));

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToSaveStateException))]
        public async Task WhenStorageIsBrokenThenAnAppropriateExceptionIsThrownOnStoreState()
        {
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();
            cloudBlobContainerWrapperMock
                .Setup(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                .Returns(() => throw new StorageException(new RequestResult(), string.Empty, null));

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            var state = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await blobStateRepository.StoreStateAsync("key", state, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(FailedToDeleteStateException))]
        public async Task WhenStorageIsBrokenThenAnAppropriateExceptionIsThrownOnDeleteState()
        {
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();
            cloudBlobContainerWrapperMock
                .Setup(m => m.DeleteBlobIfExistsAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns(() => throw new StorageException(new RequestResult(), string.Empty, null));

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            var state = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await blobStateRepository.DeleteStateAsync("key", CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(StateSerializationException))]
        public async Task WhenStateIsUnserializableThenAnAppropriateExceptionIsThrownOnStoreState()
        {
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            var state = new UnserializableState
            {
                Property = "Hello"
            };

            await blobStateRepository.StoreStateAsync("key", state, CancellationToken.None);
        }

        [TestMethod]
        public async Task WhenExecutingBasicStateActionsThenFlowCompletesSuccesfully()
        {
            Dictionary<string, string> repository = new Dictionary<string, string>();
            Mock<ICloudBlobContainerWrapper> cloudBlobContainerWrapperMock = new Mock<ICloudBlobContainerWrapper>();
            cloudBlobContainerWrapperMock
                .Setup(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                .Returns<string, string, CancellationToken>((blobName, blobContent, token) =>
                {
                    repository[blobName] = blobContent;
                    return Task.FromResult<ICloudBlob>(null);
                });
            cloudBlobContainerWrapperMock
                .Setup(m => m.DownloadBlobContentAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns<string, CancellationToken>((blobName, token) =>
                {
                    if (repository.ContainsKey(blobName))
                    {
                        return Task.FromResult<string>(repository[blobName]);
                    }
                    else
                    {
                        StorageException ex = new StorageException(new RequestResult(), string.Empty, null);
                        ex.RequestInformation.HttpStatusCode = (int)HttpStatusCode.NotFound;
                        throw ex;
                    }
                });
            cloudBlobContainerWrapperMock
                .Setup(m => m.DeleteBlobIfExistsAsync(It.IsAny<string>(), CancellationToken.None))
                .Returns<string, CancellationToken>((blobName, token) =>
                {
                    repository.Remove(blobName);
                    return Task.CompletedTask;
                });

            Mock<ICloudStorageProviderFactory> cloudStorageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            cloudStorageProviderFactoryMock.Setup(m => m.GetSmartDetectorStateStorageContainerAsync())
                .ReturnsAsync(cloudBlobContainerWrapperMock.Object);

            BlobStateRepository blobStateRepository = new BlobStateRepository("TestSmartDetector", "TestAlertRuleResourceID", cloudStorageProviderFactoryMock.Object, (new Mock<IExtendedTracer>()).Object);

            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await blobStateRepository.StoreStateAsync("key", originalState, CancellationToken.None);

            var retrievedState = await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // validate
            Assert.AreEqual(originalState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, retrievedState.Field2);
            Assert.AreEqual(originalState.Field3, retrievedState.Field3);
            Assert.AreEqual(originalState.Field4, retrievedState.Field4);
        }

        private static async Task TestFullFlow(IStateRepository blobStateRepository)
        {
            var originalState = new TestState
            {
                Field1 = "testdata",
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            // get non existing state, should return null
            var retrievedState = await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);
            Assert.IsNull(retrievedState);

            await blobStateRepository.StoreStateAsync("key", originalState, CancellationToken.None);

            retrievedState = await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // validate
            Assert.AreEqual(originalState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(originalState.Field2, retrievedState.Field2);
            Assert.AreEqual(originalState.Field3, retrievedState.Field3);
            Assert.AreEqual(originalState.Field4, retrievedState.Field4);

            // update existing state
            var updatedState = new TestState
            {
                Field1 = null,
                Field2 = new List<DateTime> { new DateTime(2018, 02, 15) },
                Field3 = true
            };

            await blobStateRepository.StoreStateAsync("key", updatedState, CancellationToken.None);

            retrievedState = await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            // validate
            Assert.AreEqual(updatedState.Field1, retrievedState.Field1);
            CollectionAssert.AreEquivalent(updatedState.Field2, retrievedState.Field2);
            Assert.AreEqual(updatedState.Field3, retrievedState.Field3);
            Assert.AreEqual(updatedState.Field4, retrievedState.Field4);

            await blobStateRepository.StoreStateAsync("key2", originalState, CancellationToken.None);
            await blobStateRepository.DeleteStateAsync("key", CancellationToken.None);

            // clear again, should not throw
            await blobStateRepository.DeleteStateAsync("key", CancellationToken.None);

            retrievedState = await blobStateRepository.GetStateAsync<TestState>("key", CancellationToken.None);

            Assert.IsNull(retrievedState);

            retrievedState = await blobStateRepository.GetStateAsync<TestState>("key2", CancellationToken.None);

            Assert.IsNotNull(retrievedState);
        }

        private class TestState
        {
            public string Field1 { get; set; }

            public List<DateTime> Field2 { get; set; }

            public bool Field3 { get; set; }

            public string Field4 { get; set; }
        }

        private class UnserializableState
        {
            public UnserializableState()
            {
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test code, approved")]
            public string Property
            {
                get { throw new NotImplementedException(); }
                set { }
            }
        }
    }
}
