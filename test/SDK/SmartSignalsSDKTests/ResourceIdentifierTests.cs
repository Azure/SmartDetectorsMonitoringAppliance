//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsSDKTests
{
    using System;
    using Microsoft.Azure.Monitoring.SmartSignals;
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
        public void WhenCreatingSubscriptionResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest(ResourceIdentifier.Create);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingSubscriptionResourceWithWrongConstructorThenExceptionIsThrown()
        {
            var unused = ResourceIdentifier.Create(ResourceType.Subscription, TestSubscriptionId, TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        public void WhenCreatingResourceGroupResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((subscriptionId) => ResourceIdentifier.Create(subscriptionId, TestResourceGroup));
        }

        [TestMethod]
        public void WhenCreatingResourceGroupResourceIdentifierWithEmptyResourceGroupNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceGroupName) => ResourceIdentifier.Create(TestSubscriptionId, resourceGroupName));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingResourceGroupResourceIdentifierWithWrongConstructorThenExceptionIsThrown()
        {
            var unused = ResourceIdentifier.Create(ResourceType.ResourceGroup, TestSubscriptionId, TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((subscriptionId) => ResourceIdentifier.Create(ResourceType.VirtualMachine, subscriptionId, TestResourceGroup, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptyResourceGroupNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceGroupName) => ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, resourceGroupName, TestResourceName));
        }

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierWithEmptyResourceNameThenExceptionIsThrown()
        {
            InvalidEmptyParameterTest((resourceName) => ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, resourceName));
        }

        #endregion

        #region Constructors tests

        [TestMethod]
        public void WhenCreatingVmResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            Assert.AreEqual(ResourceType.VirtualMachine, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(TestResourceGroup, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(TestResourceName, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingSubscriptionResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = ResourceIdentifier.Create(TestSubscriptionId);
            Assert.AreEqual(ResourceType.Subscription, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingResourceGroupResourceIdentifierThenPropertiesAreSet()
        {
            var resourceIdentifier = ResourceIdentifier.Create(TestSubscriptionId, TestResourceGroup);
            Assert.AreEqual(ResourceType.ResourceGroup, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(TestResourceGroup, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(string.Empty, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenDeserializingResourceIdentifierObjectThenTheSerializationConstructorIsCalledAndThePropertiesAreSet()
        {
            ResourceIdentifier resourceIdentifier = ResourceIdentifier.Create(TestSubscriptionId);
            string json = JsonConvert.SerializeObject(resourceIdentifier);
            ResourceIdentifier resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);

            resourceIdentifier = ResourceIdentifier.Create(TestSubscriptionId, TestResourceGroup);
            json = JsonConvert.SerializeObject(resourceIdentifier);
            resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);

            resourceIdentifier = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            json = JsonConvert.SerializeObject(resourceIdentifier);
            resourceIdentifier2 = JsonConvert.DeserializeObject<ResourceIdentifier>(json);
            Assert.AreEqual(resourceIdentifier, resourceIdentifier2);
        }

        #endregion

        #region Equality tests

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithTheSameDataThenTheyAreEqual()
        {
            var first = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);

            Assert.IsTrue(first.Equals(second), "Expected both identifiers to be equal");
            Assert.IsTrue(first == second, "Expected both identifiers to be equal using equality comparison");
            Assert.IsFalse(first != second, "Expected both identifiers to be equal using inequality comparison");
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentTypesThenTheyAreNotEqual()
        {
            var first = ResourceIdentifier.Create(TestSubscriptionId);
            var second = ResourceIdentifier.Create(TestSubscriptionId, TestResourceGroup);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentSubscriptionIdsThenTheyAreNotEqual()
        {
            var first = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = ResourceIdentifier.Create(ResourceType.VirtualMachine, "otherSubscription", TestResourceGroup, TestResourceName);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentResourceGroupsThenTheyAreNotEqual()
        {
            var first = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, "otherResourceGroup", TestResourceName);

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentResourceNamesThenTheyAreNotEqual()
        {
            var first = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = ResourceIdentifier.Create(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, "otherResource");

            Assert.IsFalse(first.Equals(second), "Expected both identifiers to be not equal");
            Assert.IsFalse(first == second, "Expected both identifiers to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both identifiers to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have not equal hash codes");
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

        #endregion
    }
}
