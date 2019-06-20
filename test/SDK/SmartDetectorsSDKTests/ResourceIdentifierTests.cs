//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSDKTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ResourceIdentifierTests
    {
        private const string TestSubscriptionId = "subscriptionId";
        private const string TestResourceGroup = "resourceGroup";
        private const string TestResourceName = "resourceName";

        #region Error cases

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((subscriptionId) => new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, TestResourceGroup, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptyResourceGroupNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceGroupName) => new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, resourceGroupName, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptyResourceNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceName) => new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, resourceName));
        }

        [TestMethod]
        public void WhenCreatingAKSResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((subscriptionId) => new ResourceIdentifier(ResourceType.KubernetiesService, subscriptionId, TestResourceGroup, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingAKSResourceIdentifierWithEmptyResourceGroupNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceGroupName) => new ResourceIdentifier(ResourceType.KubernetiesService, TestSubscriptionId, resourceGroupName, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingAKSResourceIdentifierWithEmptyResourceNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceName) => new ResourceIdentifier(ResourceType.KubernetiesService, TestSubscriptionId, TestResourceGroup, resourceName));
        }

        #endregion

        #region Constructors tests

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            Assert.AreEqual(ResourceType.VirtualMachine, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(TestResourceGroup, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(TestResourceName, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingSubscriptionResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, string.Empty, string.Empty);
            Assert.AreEqual(ResourceType.Subscription, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingResourceGroupResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, TestResourceGroup, string.Empty);
            Assert.AreEqual(ResourceType.ResourceGroup, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(TestResourceGroup, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenDeserializingResourceIdentifierObjectThenTheSerializationConstructorIsCalledAndThePropertiesAreSet()
        {
            ResourceIdentifier resourceIdentifier = new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, string.Empty, string.Empty);
            string json = JsonConvert.SerializeObject(resourceIdentifier);
            ResourceIdentifier resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);

            resourceIdentifier = new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, TestResourceGroup, string.Empty);
            json = JsonConvert.SerializeObject(resourceIdentifier);
            resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);

            resourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            json = JsonConvert.SerializeObject(resourceIdentifier);
            resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);

            resourceIdentifier = new ResourceIdentifier(ResourceType.KubernetiesService, TestSubscriptionId, TestResourceGroup, TestResourceName);
            json = JsonConvert.SerializeObject(resourceIdentifier);
            resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);
        }

        #endregion

        #region Equality tests

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithTheSameDataThenTheyAreEqual()
        {
            var first = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);

            Assert.IsTrue(first.Equals(second), "Expected both identifiers to be equal");
            Assert.IsTrue(first == second, "Expected both identifiers to be equal using equality comparison");
            Assert.IsFalse(first != second, "Expected both identifiers to be equal using inequality comparison");
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have equal hash codes");

            var firstAKS = new ResourceIdentifier(ResourceType.KubernetiesService, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var secondAKS = new ResourceIdentifier(ResourceType.KubernetiesService, TestSubscriptionId, TestResourceGroup, TestResourceName);

            Assert.IsTrue(firstAKS.Equals(secondAKS), "Expected both identifiers to be equal");
            Assert.IsTrue(firstAKS == secondAKS, "Expected both identifiers to be equal using equality comparison");
            Assert.IsFalse(firstAKS != secondAKS, "Expected both identifiers to be equal using inequality comparison");
            Assert.AreEqual(firstAKS.GetHashCode(), secondAKS.GetHashCode(), "Expected both identifiers have equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentTypesThenTheyAreNotEqual()
        {
            var first = new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, string.Empty, string.Empty);
            var second = new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, TestResourceGroup, string.Empty);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentSubscriptionIdsThenTheyAreNotEqual()
        {
            var first = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.VirtualMachine, "otherSubscription", TestResourceGroup, TestResourceName);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentResourceGroupsThenTheyAreNotEqual()
        {
            var first = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, "otherResourceGroup", TestResourceName);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentResourceNamesThenTheyAreNotEqual()
        {
            var first = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, "otherResource");

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenConvertingVmResourceTheConversionIsSuccessful()
        {
            string testResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName";
            ResourceIdentifier testResourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, "7904b7bd-5e6b-4415-99a8-355657b7da19", "MyResourceGroupName", "MyVirtualMachineName");
            VerifyConversion(testResourceId, testResourceIdentifier);
        }

        [TestMethod]
        public void WhenConvertingAKSResourceTheConversionIsSuccessful()
        {
            string testResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.ContainerService/managedClusters/MyVirtualMachineName";
            ResourceIdentifier testResourceIdentifier = new ResourceIdentifier(ResourceType.KubernetiesService, "7904b7bd-5e6b-4415-99a8-355657b7da19", "MyResourceGroupName", "MyVirtualMachineName");
            VerifyConversion(testResourceId, testResourceIdentifier);
        }

        [TestMethod]
        public void WhenCallingToResourceIdTheDictionaryConversionIsSuccessful()
        {
            List<ResourceType> resources = ((ResourceType[])Enum.GetValues(typeof(ResourceType)))
                .Except(new List<ResourceType>() { ResourceType.Subscription, ResourceType.ResourceGroup }).ToList();

            foreach (ResourceType resourceType in resources)
            {
                ResourceIdentifier testResourceIdentifier = new ResourceIdentifier(resourceType, "7904b7bd-5e6b-4415-99a8-355657b7da19", "MyResourceGroupName", "MyVirtualMachineName");
                var resourceIdentifier = testResourceIdentifier.ToResourceId();
            }
        }

        #endregion

        #region ToResourceId tests

        [TestMethod]
        public void WhenGettingResourceIdForAzureStorageResourcesWithSupportedServiceTypeThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");

            string azureStorageBlobResourceId = resourceIdentifier.ToResourceId(StorageServiceType.Blob);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/blobServices/default", azureStorageBlobResourceId, "incorrect resource Id was generated for a given resource");

            string azureStorageFileResourceId = resourceIdentifier.ToResourceId(StorageServiceType.File);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/fileServices/default", azureStorageFileResourceId, "incorrect resource Id was generated for a given resource");

            string azureStorageQueueResourceId = resourceIdentifier.ToResourceId(StorageServiceType.Queue);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/queueServices/default", azureStorageQueueResourceId, "incorrect resource Id was generated for a given resource");

            string azureStorageTableResourceId = resourceIdentifier.ToResourceId(StorageServiceType.Table);
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME/tableServices/default", azureStorageTableResourceId, "incorrect resource Id was generated for a given resource");
        }

        [TestMethod]
        public void WhenGettingResourceIdForAzureStorageResourcesWithNonSupportedServiceTypeThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");

            string azureStorageWithNonSupportedServiceTypeResourceId = resourceIdentifier.ToResourceId();
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/STORAGE_NAME", azureStorageWithNonSupportedServiceTypeResourceId, "incorrect resource Id was generated for a given resource");
        }

        [TestMethod]
        public void WhenGettingResourceIdForNonAzureStorageResourcesThenTheCorrectUrlWasCreated()
        {
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.KeyVault,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "KEYVAULT_NAME");

            string keyVaultResourceId = resourceIdentifier.ToResourceId();
            Assert.AreEqual("/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.KeyVault/vaults/KEYVAULT_NAME", keyVaultResourceId, "incorrect resource Id was generated for a given resource");
        }

        #endregion

        #region Private methods

        private static void InvalidEmptyParameterTest(Func<string, ResourceIdentifier> function)
        {
            // Try to create a ResourceIdentifier with the specified parameters, testing null strings will all 3 options (null, empty, or whitespace).
            // The creation is expected to always fail with ArgumentNullException.
            foreach (string empty in new[] { null, string.Empty, "   " })
            {
                try
                {
                    var unused = function(empty);
                    Assert.Fail("Creation of resource identifier should have failed");
                }
                catch (ArgumentNullException)
                {
                    // This exception should have been thrown
                }
            }
        }

        private static void VerifyConversion(string testResourceId, ResourceIdentifier testResourceIdentifier)
        {
            var resourceIdentifier = ResourceIdentifier.CreateFromResourceId(testResourceId);
            var resourceId = resourceIdentifier.ToResourceId();
            Assert.AreEqual(testResourceId, resourceId, "Resource IDs are different");
            Assert.AreEqual(testResourceIdentifier, resourceIdentifier, "Resource identifiers are are different");

            resourceId = testResourceIdentifier.ToResourceId();
            resourceIdentifier = ResourceIdentifier.CreateFromResourceId(resourceId);
            Assert.AreEqual(testResourceId, resourceId, "Resource IDs are different");
            Assert.AreEqual(testResourceIdentifier, resourceIdentifier, "Resource identifiers are are different");
        }
        #endregion
    }
}
