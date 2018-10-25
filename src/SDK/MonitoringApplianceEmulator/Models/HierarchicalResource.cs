//-----------------------------------------------------------------------
// <copyright file="HierarchicalResource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an hierarchical Azure resource.
    /// </summary>
    public class HierarchicalResource : ObservableObject
    {
        private FilterableCollection<HierarchicalResource> containedResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchicalResource"/> class.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier</param>
        /// <param name="containedResources">The Azure resources contained by the resource</param>
        /// <param name="name">The resource name</param>
        public HierarchicalResource(ResourceIdentifier resourceIdentifier, List<HierarchicalResource> containedResources, string name)
        {
            this.ResourceIdentifier = resourceIdentifier;
            this.ContainedResources = new FilterableCollection<HierarchicalResource>(containedResources);

            // In case of a filter change in contained resources collection, update all contained resources with the new filter as well
            this.ContainedResources.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(this.ContainedResources.Filter))
                {
                    foreach (HierarchicalResource containedResource in this.ContainedResources.OriginalCollection)
                    {
                        containedResource.ContainedResources.Filter = this.ContainedResources.Filter;
                    }
                }
            };

            this.Name = name;
        }

        /// <summary>
        /// Gets the resource identifier.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier { get; }

        /// <summary>
        /// Gets the Azure resources contained by the resource.
        /// </summary>
        public FilterableCollection<HierarchicalResource> ContainedResources
        {
            get
            {
                return this.containedResources;
            }

            private set
            {
                this.containedResources = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the resource name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Finds the first Azure resource that exists in the hierarchy's current *filtered* collections.
        /// </summary>
        /// <param name="resourceName">The name of the resource to find.</param>
        /// <returns>The resource, or <code>null</code> if doesn't exist</returns>
        public HierarchicalResource TryFind(string resourceName)
        {
            if (resourceName == null)
            {
                return null;
            }

            if (this.Name == resourceName)
            {
                return this;
            }

            return this.ContainedResources.FilteredCollection
                .Select(resource => resource.TryFind(resourceName))
                .FirstOrDefault(resource => resource != null);
        }
    }
}