//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierExtensionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests
{
    using System;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResourceIdentifierExtensionsTests
    {
        [TestMethod]
        public void AllServiceTypesExistInMappingDictionary()
        {
            foreach (ServiceType serviceType in Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Where(t => t != ServiceType.None))
            {
                Assert.IsTrue(ResourceIdentifierExtensions.MapAzureServiceTypeToPresentationInUri.Keys.Contains(serviceType), $"Service {serviceType} is missing in service mapping dictionary");
            }
        }

        [TestMethod]
        public void WhenGettingUrlForAzureStorageResourcesWithSupportedServiceTypeThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");

            string azureStorageBlobUri = resourceIdentifier.GetResourceFullUri(ServiceType.AzureStorageBlob);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/blobServices/default", azureStorageBlobUri, "incorrect uri was generated for a given resource");

            string azureStorageFileUri = resourceIdentifier.GetResourceFullUri(ServiceType.AzureStorageFile);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/fileServices/default", azureStorageFileUri, "incorrect uri was generated for a given resource");

            string azureStorageQueueUri = resourceIdentifier.GetResourceFullUri(ServiceType.AzureStorageQueue);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/queueServices/default", azureStorageQueueUri, "incorrect uri was generated for a given resource");

            string azureStorageTableUri = resourceIdentifier.GetResourceFullUri(ServiceType.AzureStorageTable);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/tableServices/default", azureStorageTableUri, "incorrect uri was generated for a given resource");
        }

        [TestMethod]
        public void WhenGettingUrlForAzureStorageResourcesWithNonSupportedServiceTypeThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");

            string azureStorageWithNonSupportedServiceTypeUri = resourceIdentifier.GetResourceFullUri(ServiceType.None);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME", azureStorageWithNonSupportedServiceTypeUri, "incorrect uri was generated for a given resource");
        }

        [TestMethod]
        public void WhenGettingUrlForNonAzureStorageResourcesThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.KeyVault,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "KEYVAULT_NAME");

            string keyVaultUri = resourceIdentifier.GetResourceFullUri(ServiceType.None);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.KeyVault/vaults/KEYVAULT_NAME", keyVaultUri, "incorrect uri was generated for a given resource");
        }
    }
}
