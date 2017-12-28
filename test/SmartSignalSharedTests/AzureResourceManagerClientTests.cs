//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerClientTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSharedTests
{
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AzureResourceManagerClientTests
    {
        [TestMethod]
        public void WhenConvertingSubscriptionResourceTheConversionIsSuccessful()
        {
            string testResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19";
            ResourceIdentifier testResourceIdentifier = ResourceIdentifier.Create("7904b7bd-5e6b-4415-99a8-355657b7da19");
            this.VerifyConversion(testResourceId, testResourceIdentifier);
        }

        [TestMethod]
        public void WhenConvertingResourceGroupResourceTheConversionIsSuccessful()
        {
            string testResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName";
            ResourceIdentifier testResourceIdentifier = ResourceIdentifier.Create("7904b7bd-5e6b-4415-99a8-355657b7da19", "MyResourceGroupName");
            this.VerifyConversion(testResourceId, testResourceIdentifier);
        }

        [TestMethod]
        public void WhenConvertingVmResourceTheConversionIsSuccessful()
        {
            string testResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName";
            ResourceIdentifier testResourceIdentifier = ResourceIdentifier.Create(ResourceType.VirtualMachine, "7904b7bd-5e6b-4415-99a8-355657b7da19", "MyResourceGroupName", "MyVirtualMachineName");
            this.VerifyConversion(testResourceId, testResourceIdentifier);
        }

        private void VerifyConversion(string testResourceId, ResourceIdentifier testResourceIdentifier)
        {
            IAzureResourceManagerClient client = new AzureResourceManagerClient();

            var resourceIdentifier = client.GetResourceIdentifier(testResourceId);
            var resourceId = client.GetResourceId(resourceIdentifier);
            Assert.AreEqual(testResourceId, resourceId, "Resource IDs are different");
            Assert.AreEqual(testResourceIdentifier, resourceIdentifier, "Resource identifiers are are different");

            resourceId = client.GetResourceId(testResourceIdentifier);
            resourceIdentifier = client.GetResourceIdentifier(resourceId);
            Assert.AreEqual(testResourceId, resourceId, "Resource IDs are different");
            Assert.AreEqual(testResourceIdentifier, resourceIdentifier, "Resource identifiers are are different");
        }
    }
}