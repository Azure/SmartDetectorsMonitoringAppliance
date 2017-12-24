//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifier.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// A representation of the identity a specific resource in Azure.
    /// </summary>
    public struct ResourceIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceIdentifier"/> structure.
        /// This constructor performs all necessary parameter validations, and is called
        /// from each of the ResourceIdentifier.Create methods.
        /// It is only called directly by JSON serialization.
        /// </summary>
        /// <param name="resourceType">The resource's type.</param>
        /// <param name="subscriptionId">The ID of the subscription the resource belongs to.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource belongs to.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <exception cref="ArgumentNullException">One of the string parameters that should not be empty, is empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the string parameters that should be empty, is not empty</exception>
        [JsonConstructor]
        private ResourceIdentifier(ResourceType resourceType, string subscriptionId, string resourceGroupName, string resourceName)
        {
            // Parameter validations
            if (resourceType == ResourceType.Subscription)
            {
                // Validate that the subscriptionId is not empty, and that the resourceGroupName and resourceName are empty
                if (string.IsNullOrWhiteSpace(subscriptionId))
                {
                    throw new ArgumentNullException(nameof(subscriptionId), "The subscription's ID cannot be empty");
                }

                if (!string.IsNullOrWhiteSpace(resourceGroupName))
                {
                    throw new ArgumentOutOfRangeException(nameof(resourceGroupName), "The subscription's resource group name must be empty");
                }

                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    throw new ArgumentOutOfRangeException(nameof(resourceName), "The subscription's resource name must be empty");
                }

                resourceGroupName = string.Empty;
                resourceName = string.Empty;
            }
            else if (resourceType == ResourceType.ResourceGroup)
            {
                // Validate that the subscriptionId and resourceGroupName are not empty, and that the resourceName is empty
                if (string.IsNullOrWhiteSpace(subscriptionId))
                {
                    throw new ArgumentNullException(nameof(subscriptionId), "The resource group's subscription ID cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(resourceGroupName))
                {
                    throw new ArgumentNullException(nameof(resourceGroupName), "The resource group name cannot be empty");
                }

                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    throw new ArgumentOutOfRangeException(nameof(resourceName), "The resource group's resource name must be empty");
                }

                resourceName = string.Empty;
            }
            else
            {
                // Validate that the subscriptionId, resourceGroupName, and resourceName are not empty
                if (string.IsNullOrWhiteSpace(subscriptionId))
                {
                    throw new ArgumentNullException(nameof(subscriptionId), "The resource's subscription ID cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(resourceGroupName))
                {
                    throw new ArgumentNullException(nameof(resourceGroupName), "The resource's resource group name cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    throw new ArgumentNullException(nameof(resourceName), "The resource's name cannot be empty");
                }
            }

            this.ResourceType = resourceType;
            this.SubscriptionId = subscriptionId;
            this.ResourceGroupName = resourceGroupName;
            this.ResourceName = resourceName;
        }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        public ResourceType ResourceType { get; }

        /// <summary>
        /// Gets the ID of the subscription the resource belongs to.
        /// </summary>
        public string SubscriptionId { get; }

        /// <summary>
        /// Gets the name of the resource group the resource belongs to.
        /// This can be <code>null</code> if the resource is a subscription or
        /// resource group.
        /// </summary>
        public string ResourceGroupName { get; }

        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceIdentifier"/> structure,
        /// representing a resource of type <see cref="SmartSignals.ResourceType.Subscription"/>.
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <exception cref="ArgumentNullException">The subscription ID is empty.</exception>
        /// <returns>A new instance of the <see cref="ResourceIdentifier"/> structure</returns>
        public static ResourceIdentifier Create(string subscriptionId)
        {
            return new ResourceIdentifier(ResourceType.Subscription, subscriptionId, string.Empty, string.Empty);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceIdentifier"/> structure,
        /// representing a resource of type <see cref="SmartSignals.ResourceType.ResourceGroup"/>.
        /// </summary>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <exception cref="ArgumentNullException">The subscription ID or resource group name is empty.</exception>
        /// <returns>A new instance of the <see cref="ResourceIdentifier"/> structure</returns>
        public static ResourceIdentifier Create(string subscriptionId, string resourceGroupName)
        {
            return new ResourceIdentifier(ResourceType.ResourceGroup, subscriptionId, resourceGroupName, string.Empty);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceIdentifier"/> structure,
        /// representing a specific Azure resource.
        /// This method should not be used to created a <see cref="ResourceIdentifier"/>
        /// structure of type <see cref="SmartSignals.ResourceType.Subscription"/> or of type
        /// <see cref="SmartSignals.ResourceType.ResourceGroup"/> - use the other specialized
        /// ResourceIdentifier.Create methods to create strictures of these resource types.
        /// </summary>
        /// <param name="resourceType">The resource's type.</param>
        /// <param name="subscriptionId">The ID of the subscription the resource belongs to.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource belongs to.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="subscriptionId"/>, <paramref name="resourceGroupName"/>,
        /// or <paramref name="resourceName"/> are empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="resourceType"/> is either <see cref="SmartSignals.ResourceType.Subscription"/>
        /// or <see cref="SmartSignals.ResourceType.ResourceGroup"/>.
        /// </exception> 
        /// <returns>A new instance of the <see cref="ResourceIdentifier"/> structure</returns>
        public static ResourceIdentifier Create(ResourceType resourceType, string subscriptionId, string resourceGroupName, string resourceName)
        {
            if (resourceType == ResourceType.Subscription || resourceType == ResourceType.ResourceGroup)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceType), "The resource type cannot be Subscription or ResourceGroup");
            }

            return new ResourceIdentifier(resourceType, subscriptionId, resourceGroupName, resourceName);
        }

        #region Overrides of ValueType

        /// <summary>
        /// Determines whether two specified source identifiers have the same value.
        /// </summary>
        /// <param name="a">The first resource identifier to compare.</param>
        /// <param name="b">The second resource identifier to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> represent the same resource identifier; otherwise, false.</returns>
        public static bool operator ==(ResourceIdentifier a, ResourceIdentifier b)
        {
            return
                a.ResourceType == b.ResourceType &&
                a.SubscriptionId == b.SubscriptionId &&
                a.ResourceGroupName == b.ResourceGroupName &&
                a.ResourceName == b.ResourceName;
        }

        /// <summary>
        /// Determines whether two specified source identifiers have different values.
        /// </summary>
        /// <param name="a">The first resource identifier to compare.</param>
        /// <param name="b">The second resource identifier to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> do not represent the same resource identifier; otherwise, false.</returns>
        public static bool operator !=(ResourceIdentifier a, ResourceIdentifier b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns>
        /// true if <paramref name="value"/> is an instance of <see cref="ResourceIdentifier"/> and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object value)
        {
            return value is ResourceIdentifier identifier && this == identifier;
        }

        /// <summary>
        /// Returns the hash code for this resource identifier.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            // Disable overflow - just in case
            unchecked
            {
                int hash = 27;
                hash = (31 * hash) + this.ResourceType.GetHashCode();
                hash = (31 * hash) + this.SubscriptionId.GetHashCode();
                hash = (31 * hash) + (this.ResourceGroupName?.GetHashCode() ?? 0);
                hash = (31 * hash) + (this.ResourceName?.GetHashCode() ?? 0);
                return hash;
            }
        }

        #endregion
    }
}
