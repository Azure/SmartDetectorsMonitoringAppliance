namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;

    /// <summary>
    /// A representation of the identity a specific resource in Azure.
    /// </summary>
    public struct ResourceIdentifier
    {
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
        /// Initializes a new instance of the <see cref="ResourceIdentifier"/> structure.
        /// </summary>
        /// <param name="resourceType">The resource's type.</param>
        /// <param name="subscriptionId">The ID of the subscription the resource belongs to.</param>
        /// <param name="resourceGroupName">
        /// The name of the resource group the resource belongs to.
        /// This can be <code>null</code> if the resource is a subscription or
        /// resource group.
        /// </param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="subscriptionId"/> or <paramref name="resourceName"/> are empty, or if
        /// <paramref name="resourceGroupName"/> is empty and the resource is not a subscription or resource group
        /// resource.
        /// </exception>
        public ResourceIdentifier(ResourceType resourceType, string subscriptionId, string resourceGroupName, string resourceName)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException(nameof(subscriptionId), "A resource's subscription ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName), "A resource's name cannot be empty");
            }

            if (resourceType != ResourceType.Subscription &&
                resourceType != ResourceType.ResourceGroup &&
                string.IsNullOrWhiteSpace(resourceGroupName))
            {
                throw new ArgumentNullException(
                    nameof(resourceGroupName), 
                    $"A resource's resource group name cannot be empty for resources of type {resourceType}");
            }

            this.ResourceType = resourceType;
            this.SubscriptionId = subscriptionId;
            this.ResourceGroupName = resourceGroupName;
            this.ResourceName = resourceName;
        }

        #region Overrides of ValueType

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns>
        /// true if <paramref name="value"/> is an instance of <see cref="ResourceIdentifier"/> and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object value)
        {
            return value is ResourceIdentifier && this == (ResourceIdentifier)value;
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
                hash = (31 * hash) + this.ResourceName.GetHashCode();
                return hash;
            }
        }

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

        #endregion
    }
}
