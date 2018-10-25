//-----------------------------------------------------------------------
// <copyright file="ResourceSku.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Arm
{
    /// <summary>
    /// A class holding SKU information for an ARM resource
    /// </summary>
    public class ResourceSku
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSku"/> class
        /// </summary>
        /// <param name="name">The SKU name</param>
        /// <param name="tier">The SKU pricing tier</param>
        /// <param name="size">The SKU size</param>
        /// <param name="family">The SKU family</param>
        /// <param name="model">The SKU model</param>
        /// <param name="capacity">The SKU capacity</param>
        public ResourceSku(string name, string tier, string size, string family, string model, int? capacity)
        {
            this.Name = name;
            this.Tier = tier;
            this.Size = size;
            this.Family = family;
            this.Model = model;
            this.Capacity = capacity;
        }

        /// <summary>
        /// Gets the SKU name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the SKU pricing tier
        /// </summary>
        public string Tier { get; }

        /// <summary>
        /// Gets the SKU size
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Gets the SKU family
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// Gets the SKU model
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// Gets the SKU capacity
        /// </summary>
        public int? Capacity { get; }
    }
}