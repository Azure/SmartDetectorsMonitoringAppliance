//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifier.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// A representation of the identity a specific resource in Azure.
    /// </summary>
    public struct ResourceIdentifier : IEquatable<ResourceIdentifier>
    {
        /// <summary>
        /// A dictionary, mapping <see cref="ResourceType"/> enumeration values to matching ARM string
        /// </summary>
        public static readonly ReadOnlyDictionary<ResourceType, string> MapResourceTypeToString =
            new ReadOnlyDictionary<ResourceType, string>(
                new Dictionary<ResourceType, string>()
                {
                    [ResourceType.VirtualMachine] = "Microsoft.Compute/virtualMachines",
                    [ResourceType.VirtualMachineScaleSet] = "Microsoft.Compute/virtualMachineScaleSets",
                    [ResourceType.ApplicationInsights] = "Microsoft.Insights/components",
                    [ResourceType.LogAnalytics] = "Microsoft.OperationalInsights/workspaces",
                    [ResourceType.AzureStorage] = "Microsoft.Storage/storageAccounts",
                    [ResourceType.CosmosDb] = "Microsoft.DocumentDB/databaseAccounts",
                    [ResourceType.KeyVault] = "Microsoft.KeyVault/vaults",
                    [ResourceType.ServiceBus] = "Microsoft.ServiceBus/namespaces",
                    [ResourceType.SqlServer] = "Microsoft.Sql/servers",
                    [ResourceType.EventHub] = "Microsoft.EventHub/namespaces",
                });

        private const string SubscriptionRegexPattern = "/subscriptions/(?<subscriptionId>[^/]*)";
        private const string ResourceGroupRegexPattern = SubscriptionRegexPattern + "/resourceGroups/(?<resourceGroupName>[^/]*)";
        private const string ResourceRegexPattern = ResourceGroupRegexPattern + "/providers/(?<resourceProviderAndType>.*)/(?<resourceName>[^/]*)";

        /// <summary>
        /// A dictionary, mapping ARM strings to their matching <see cref="ResourceType"/> enumeration values
        /// </summary>
        private static readonly Dictionary<string, ResourceType> MapStringToResourceType = MapResourceTypeToString.ToDictionary(x => x.Value, x => x.Key, StringComparer.CurrentCultureIgnoreCase);

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
        public ResourceIdentifier(ResourceType resourceType, string subscriptionId, string resourceGroupName, string resourceName)
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
        [JsonProperty("resourceType")]
        public ResourceType ResourceType { get; }

        /// <summary>
        /// Gets the ID of the subscription the resource belongs to.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; }

        /// <summary>
        /// Gets the name of the resource group the resource belongs to.
        /// This can be <code>null</code> if the resource is a subscription or
        /// resource group.
        /// </summary>
        [JsonProperty("resourceGroupName")]
        public string ResourceGroupName { get; }

        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        [JsonProperty("resourceName")]
        public string ResourceName { get; }

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
                string.Equals(a.SubscriptionId, b.SubscriptionId, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(a.ResourceGroupName, b.ResourceGroupName, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(a.ResourceName, b.ResourceName, StringComparison.InvariantCultureIgnoreCase);
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

        /// <summary>
        /// Creates a new instance of the <see cref="ResourceIdentifier"/> structure that represents the resource identified by the <paramref name="resourceId"/>.
        /// The <paramref name="resourceId"/> parameter is expected to be in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <param name="resourceId">The resource ID</param>
        /// <returns>The <see cref="ResourceIdentifier"/> structure.</returns>
        public static ResourceIdentifier CreateFromResourceId(string resourceId)
        {
            // Match resource pattern
            Match m = Regex.Match(resourceId, ResourceRegexPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                // Verify that the resource is of a supported type
                string resourceProviderAndType = m.Groups["resourceProviderAndType"].Value;
                if (!MapStringToResourceType.TryGetValue(resourceProviderAndType, out ResourceType resourceType))
                {
                    throw new ArgumentException($"Resource type {resourceType} is not supported.", nameof(resourceId));
                }

                return new ResourceIdentifier(resourceType, m.Groups["subscriptionId"].Value, m.Groups["resourceGroupName"].Value, m.Groups["resourceName"].Value);
            }

            // Match resource group pattern
            m = Regex.Match(resourceId, ResourceGroupRegexPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return new ResourceIdentifier(ResourceType.ResourceGroup, m.Groups["subscriptionId"].Value, m.Groups["resourceGroupName"].Value, string.Empty);
            }

            // Match subscription pattern
            m = Regex.Match(resourceId, SubscriptionRegexPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return new ResourceIdentifier(ResourceType.Subscription, m.Groups["subscriptionId"].Value, string.Empty, string.Empty);
            }

            throw new ArgumentException($"Invalid resource ID provided: {resourceId}", nameof(resourceId));
        }

        #region Overrides of ValueType

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(ResourceIdentifier other)
        {
            return this == other;
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
                hash = (31 * hash) + this.SubscriptionId?.ToUpperInvariant().GetHashCode() ?? 0;
                hash = (31 * hash) + (this.ResourceGroupName?.ToUpperInvariant().GetHashCode() ?? 0);
                hash = (31 * hash) + (this.ResourceName?.ToUpperInvariant().GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// Gets the resource ID that represents the resource identified by the specified <see cref="ResourceIdentifier"/> structure.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName
        /// </example>
        /// </summary>
        /// <returns>The resource ID.</returns>
        public string ToResourceId()
        {
            // Find the regex pattern based on the type
            string pattern;
            string resourceProviderAndType = string.Empty;
            switch (this.ResourceType)
            {
                case ResourceType.Subscription:
                    pattern = SubscriptionRegexPattern;
                    break;
                case ResourceType.ResourceGroup:
                    pattern = ResourceGroupRegexPattern;
                    break;
                default:
                    pattern = ResourceRegexPattern;
                    resourceProviderAndType = MapResourceTypeToString[this.ResourceType];
                    break;
            }

            // Replace the pattern components based on the resource identifier properties
            pattern = pattern.Replace("(?<subscriptionId>[^/]*)", this.SubscriptionId);
            pattern = pattern.Replace("(?<resourceGroupName>[^/]*)", this.ResourceGroupName);
            pattern = pattern.Replace("(?<resourceProviderAndType>.*)", resourceProviderAndType);
            pattern = pattern.Replace("(?<resourceName>[^/]*)", this.ResourceName);

            return pattern;
        }

        #endregion
    }
}
