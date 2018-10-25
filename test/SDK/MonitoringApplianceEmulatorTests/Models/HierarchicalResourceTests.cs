//-----------------------------------------------------------------------
// <copyright file="HierarchicalResourceTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HierarchicalResourceTests
    {
        private HierarchicalResource appInsightsHierarchicalResource;
        private HierarchicalResource virtualMachineHierarchicalResource;

        [TestInitialize]
        public void Setup()
        {
            ResourceIdentifier appInsightsResourceIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/microsoft.insights/components/someApp");
            this.appInsightsHierarchicalResource = new HierarchicalResource(appInsightsResourceIdentifier, new List<HierarchicalResource>(), appInsightsResourceIdentifier.ResourceName);

            ResourceIdentifier virtualMachineResourceIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName");
            this.virtualMachineHierarchicalResource = new HierarchicalResource(virtualMachineResourceIdentifier, new List<HierarchicalResource>(), virtualMachineResourceIdentifier.ResourceName);
        }

        [TestMethod]
        public void WhenCreatingNewHierarchicalResourceThenItWasInitializedCorrectly()
        {
            ResourceIdentifier resourceGroupIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName");
            List<HierarchicalResource> groupContainedResources = new List<HierarchicalResource>() { this.appInsightsHierarchicalResource, this.virtualMachineHierarchicalResource };
            var resourceGroupHierarchicalResource = new HierarchicalResource(resourceGroupIdentifier, groupContainedResources, resourceGroupIdentifier.ResourceGroupName);

            Assert.AreEqual(resourceGroupIdentifier, resourceGroupHierarchicalResource.ResourceIdentifier, "Unexpected resource identifier");
            Assert.AreEqual(resourceGroupIdentifier.ResourceGroupName, resourceGroupHierarchicalResource.Name, "Unexpected resource name");
            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.OriginalCollection, "Unexpected resource's original contained resources");
            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.FilteredCollection, "Unexpected resource's filtered contained resources");
        }

        [TestMethod]
        public void WhenFilteringContainedResourcesOfHierarchicalResourceThenTheEntireHierarchyWasFiltered()
        {
            // Create hierarchical resources structure: { subscription } -> { resourceGroup } -> { appInsights, virtualMachine }
            ResourceIdentifier resourceGroupIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName");
            List<HierarchicalResource> groupContainedResources = new List<HierarchicalResource>() { this.appInsightsHierarchicalResource, this.virtualMachineHierarchicalResource };
            var resourceGroupHierarchicalResource = new HierarchicalResource(resourceGroupIdentifier, groupContainedResources, resourceGroupIdentifier.ResourceGroupName);

            ResourceIdentifier subscriptionIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19");
            List<HierarchicalResource> subscriptionContainedResources = new List<HierarchicalResource>() { resourceGroupHierarchicalResource };
            var subscriptionHierarchicalResource = new HierarchicalResource(subscriptionIdentifier, subscriptionContainedResources, subscriptionIdentifier.SubscriptionId);

            // Verify hierarchical structure before filtering
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.FilteredCollection);

            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.FilteredCollection);

            // Filter all resources but virtualMachine
            subscriptionHierarchicalResource.ContainedResources.Filter = this.FilterAllResourcesButVirtualMachine;

            // Verify hierarchical structure after filtering
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.FilteredCollection);

            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(new List<HierarchicalResource>() { this.appInsightsHierarchicalResource }, resourceGroupHierarchicalResource.ContainedResources.FilteredCollection);
        }

        [TestMethod]
        public void WhenTryingToFindExistingResourceThenThisResourceWasFound()
        {
            // Create hierarchical resources structure: { subscription } -> { resourceGroup } -> { appInsights, virtualMachine }
            ResourceIdentifier resourceGroupIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName");
            List<HierarchicalResource> groupContainedResources = new List<HierarchicalResource>() { this.appInsightsHierarchicalResource, this.virtualMachineHierarchicalResource };
            var resourceGroupHierarchicalResource = new HierarchicalResource(resourceGroupIdentifier, groupContainedResources, resourceGroupIdentifier.ResourceGroupName);

            ResourceIdentifier subscriptionIdentifier = ResourceIdentifier.CreateFromResourceId("/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19");
            List<HierarchicalResource> subscriptionContainedResources = new List<HierarchicalResource>() { resourceGroupHierarchicalResource };
            var subscriptionHierarchicalResource = new HierarchicalResource(subscriptionIdentifier, subscriptionContainedResources, subscriptionIdentifier.SubscriptionId);

            // Verify hierarchical structure before filtering
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(subscriptionContainedResources, subscriptionHierarchicalResource.ContainedResources.FilteredCollection);

            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.OriginalCollection);
            CollectionAssert.AreEqual(groupContainedResources, resourceGroupHierarchicalResource.ContainedResources.FilteredCollection);

            Assert.IsNotNull(subscriptionHierarchicalResource.TryFind("someApp"));
            Assert.IsNotNull(subscriptionHierarchicalResource.TryFind("MyVirtualMachineName"));

            // Filter all resources but virtualMachine
            subscriptionHierarchicalResource.ContainedResources.Filter = this.FilterAllResourcesButVirtualMachine;

            Assert.IsNotNull(subscriptionHierarchicalResource.TryFind("someApp"));
            Assert.IsNull(subscriptionHierarchicalResource.TryFind("MyVirtualMachineName"));
        }

        private bool FilterAllResourcesButVirtualMachine(HierarchicalResource resource)
        {
            return resource.ResourceIdentifier.ResourceType != ResourceType.VirtualMachine;
        }
    }
}
