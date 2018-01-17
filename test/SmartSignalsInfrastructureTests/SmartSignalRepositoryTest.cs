//-----------------------------------------------------------------------
// <copyright file="SmartSignalRepositoryTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsInfrastructureTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class SmartSignalRepositoryTest
    {
        private SmartSignalRepository smartSignalRepository;
        private Mock<ICloudBlobContainerWrapper> blobContainerMock;

        [TestInitialize]
        public void Setup()
        {
            this.blobContainerMock = new Mock<ICloudBlobContainerWrapper>();
            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartSignalGlobalStorageContainer()).Returns(this.blobContainerMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.smartSignalRepository = new SmartSignalRepository(tracerMock.Object, storageProviderFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenStorageExceptionIsThrownWhenReadingAllManifestsThenCorrectExceptionIsThrown()
        {
            this.blobContainerMock.Setup(m => m.ListBlobsAsync(string.Empty, true, BlobListingDetails.Metadata)).Throws(new StorageException());

            try
            {
                await this.smartSignalRepository.ReadAllSignalsManifestsAsync();
            }
            catch (SmartSignalRepositoryException e)
            {
                if (e.InnerException is StorageException)
                {
                    return;
                }

                Assert.Fail("Exception from the blob storage should cause a repository exception with an inner exception of StorageException");
            }

            Assert.Fail("Exception from the blob storage should cause a repository exception");
        }

        [TestMethod]
        public async Task WhenStorageExceptionIsThrownWhenReadingSignalPackageThenCorrectExceptionIsThrown()
        {
            const string SignalId = "someId";
            this.blobContainerMock.Setup(m => m.ListBlobsAsync($"{SignalId}/", true, BlobListingDetails.Metadata)).Throws(new StorageException());

            try
            {
                await this.smartSignalRepository.ReadSignalPackageAsync(SignalId);
            }
            catch (SmartSignalRepositoryException e)
            {
                if (e.InnerException is StorageException)
                {
                    return;
                }

                Assert.Fail("Exception from the blob storage should cause a repository exception with an inner exception of StorageException");
            }

            Assert.Fail("Exception from the blob storage should cause a repository exception");
        }

        [TestMethod]
        public async Task WhenReadingAllSignalsManifestsThenLatestVersionsManifestsAreReturned()
        {
            const string FirstSignalId = "signalId1";
            const string SecondSignalId = "signalId2";

            var firstSignalOldVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{FirstSignalId}/signal.v1.package"));
            var firstSignalNewVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{FirstSignalId}/signal.v2.package"));
            var secondSignalOldVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{SecondSignalId}/signal.v1.package"));
            var secondSignalNewVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{SecondSignalId}/signal.v2.package"));

            var newVersionSupportedResourceTypes = new List<ResourceType> { ResourceType.ApplicationInsights };
            var oldVersionSupportedResourceTypes = new List<ResourceType> { ResourceType.ApplicationInsights, ResourceType.LogAnalytics };
            var firstSignalNewVersionMetadata = this.GenerateMetadata(FirstSignalId, "2.0", newVersionSupportedResourceTypes);
            var firstSignalOldVersionMetadata = this.GenerateMetadata(FirstSignalId, "1.0", oldVersionSupportedResourceTypes);
            var secondSignalNewMetadata = this.GenerateMetadata(SecondSignalId, "1.1.0.1", newVersionSupportedResourceTypes);
            var secondSignalOldMetadata = this.GenerateMetadata(SecondSignalId, "1.1", oldVersionSupportedResourceTypes);

            this.AssignBlobMetadata(firstSignalNewVersion, firstSignalNewVersionMetadata);
            this.AssignBlobMetadata(firstSignalOldVersion, firstSignalOldVersionMetadata);
            this.AssignBlobMetadata(secondSignalNewVersion, secondSignalNewMetadata);
            this.AssignBlobMetadata(secondSignalOldVersion, secondSignalOldMetadata);

            var blobs = new List<IListBlobItem>
            {
                firstSignalOldVersion,
                firstSignalNewVersion,
                secondSignalOldVersion,
                secondSignalNewVersion
            };

            this.blobContainerMock.Setup(m => m.ListBlobsAsync(string.Empty, true, BlobListingDetails.Metadata)).ReturnsAsync(blobs);

            var signalsManifests = await this.smartSignalRepository.ReadAllSignalsManifestsAsync();
            Assert.AreEqual(2, signalsManifests.Count);

            this.AssertMetadata(signalsManifests.First(), firstSignalNewVersionMetadata);
            this.AssertMetadata(signalsManifests.Last(), secondSignalNewMetadata);
        }

        private void AssignBlobMetadata(CloudBlockBlob blockBlob, Dictionary<string, string> metadata)
        {
            // Setting the block blob properties in reflection since it has no setter
            object attributes = blockBlob.GetType().GetField("attributes", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(blockBlob);
            attributes?.GetType().GetProperty("Metadata")?.SetValue(attributes, metadata, null);
        }

        private Dictionary<string, string> GenerateMetadata(string signalId, string version, List<ResourceType> supportedresourcetypes)
        {
            return new Dictionary<string, string>
            {
                { "id", signalId },
                { "name", "signalName" },
                { "version", version },
                { "description", "signal description" },
                { "assemblyName", "assembly" },
                { "className", "class" },
                { "supportedCadencesInMinutes", "[5,60]" },
                { "supportedResourceTypes", $"[\"{string.Join("\",\"", supportedresourcetypes)}\"]" }
            };
        }

        private void AssertMetadata(SmartSignalManifest signalManifest, Dictionary<string, string> expectedMetadata)
        {
            var supportedResourceTypes = JArray.Parse(expectedMetadata["supportedResourceTypes"])
                .Select(jtoken => (ResourceType)Enum.Parse(typeof(ResourceType), jtoken.ToString(), true))
                .ToList();

            var supportedCadencesInMinutes = JArray.Parse(expectedMetadata["supportedCadencesInMinutes"])
                .Select(jToken => int.Parse(jToken.ToString()))
                .ToList();

            Assert.AreEqual(expectedMetadata["id"], signalManifest.Id);
            Assert.AreEqual(expectedMetadata["name"], signalManifest.Name);
            Assert.AreEqual(expectedMetadata["version"], signalManifest.Version.ToString());
            Assert.AreEqual(expectedMetadata["description"], signalManifest.Description);
            Assert.AreEqual(expectedMetadata["assemblyName"], signalManifest.AssemblyName);
            Assert.AreEqual(expectedMetadata["className"], signalManifest.ClassName);

            Assert.AreEqual(supportedResourceTypes.Count, signalManifest.SupportedResourceTypes.Count);
            foreach (var supportedResourceType in supportedResourceTypes)
            {
                Assert.IsTrue(signalManifest.SupportedResourceTypes.Contains(supportedResourceType));
            }

            Assert.AreEqual(supportedCadencesInMinutes.Count, signalManifest.SupportedCadencesInMinutes.Count);
            foreach (var supportedCadence in supportedCadencesInMinutes)
            {
                Assert.IsTrue(signalManifest.SupportedCadencesInMinutes.Contains(supportedCadence));
            }
        }
    }
}
