namespace SmartSignalsSDKTests
{
    using System;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResourceIdentifierTests
    {
        private const string TestSubscriptionId = "subscriptionId";
        private const string TestResourceGroup = "resourceGroup";
        private const string TestResourceName = "resourceName";
        #region Error cases

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithNullSubscriptionIdThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, null, TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithEmptySubscriptionIdThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, string.Empty, TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithWhitespaceSubscriptionIdThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, "  ", TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithNullResourceNameThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, TestResourceGroup, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithEmptyResourceNameThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, TestResourceGroup, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithWhitespaceResourceNameThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, TestResourceGroup, "   ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithNullResourceGroupNameAndVmTypeThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, null, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithEmptyResourceGroupNameAndVmTypeThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, string.Empty, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingResourceIdentifierWithWhitespaceResourceGroupNameAndVmTypeThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, "   ", TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingResourceIdentifierWithNotEmptyResourceGroupNameAndSubscriptionTypeThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, TestResourceGroup, TestResourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingResourceIdentifierWithNotEmptyResourceGroupNameAndResourceGroupTypeThenExceptionIsThrown()
        {
            new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, TestResourceGroup, TestResourceName);
        }

        #endregion

        #region Constructors tests

        [TestMethod]
        public void WhenCreatingResourceIdentifierWithAllParametersAndVmTypeThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            Assert.AreEqual(ResourceType.VirtualMachine, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(TestResourceGroup, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(TestResourceName, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingResourceIdentifierWithEmptyResourceGroupNameAndSubscriptionTypeThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, string.Empty, TestResourceName);
            Assert.AreEqual(ResourceType.Subscription, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(null, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(TestResourceName, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        [TestMethod]
        public void WhenCreatingResourceIdentifierWithNullResourceGroupNameAndResourceGroupTypeThenPropertiesAreSet()
        {
            var resourceIdentifier = new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, string.Empty, TestResourceName);
            Assert.AreEqual(ResourceType.ResourceGroup, resourceIdentifier.ResourceType, "Mismatch on resource type");
            Assert.AreEqual(TestSubscriptionId, resourceIdentifier.SubscriptionId, "Mismatch on subscription id");
            Assert.AreEqual(null, resourceIdentifier.ResourceGroupName, "Mismatch on resource group name");
            Assert.AreEqual(TestResourceName, resourceIdentifier.ResourceName, "Mismatch on resource name");
        }

        #endregion

        #region Equality tests

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithTheDataThenTheyAreEqual()
        {
            var first = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.VirtualMachine, TestSubscriptionId, TestResourceGroup, TestResourceName);

            Assert.IsTrue(first.Equals(second), "Expected both identifiers to be equal");
            Assert.IsTrue(first == second, "Expected both identifiers to be equal using equality comparison");
            Assert.IsFalse(first != second, "Expected both identifiers to be equal using inequality comparison");
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Expected both identifiers have equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoResourceIdentifiersCreatedWithDifferentTypesThenTheyAreNotEqual()
        {
            var first = new ResourceIdentifier(ResourceType.Subscription, TestSubscriptionId, null, TestResourceName);
            var second = new ResourceIdentifier(ResourceType.ResourceGroup, TestSubscriptionId, null, TestResourceName);

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

        #endregion
    }
}
