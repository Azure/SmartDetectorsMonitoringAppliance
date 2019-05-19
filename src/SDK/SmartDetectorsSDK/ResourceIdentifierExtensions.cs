//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Extension methods for the <see cref="ResourceIdentifier"/> class
    /// </summary>
    public static class ResourceIdentifierExtensions
    {
        /// <summary>
        /// A dictionary, mapping <see cref="StorageServiceType"/> enumeration values to matching resource ID suffix strings
        /// </summary>
        private static readonly ReadOnlyDictionary<StorageServiceType, string> MapStorageServiceTypeToString =
            new ReadOnlyDictionary<StorageServiceType, string>(
                new Dictionary<StorageServiceType, string>()
                {
                    [StorageServiceType.Blob] = "blobServices/default",
                    [StorageServiceType.Table] = "tableServices/default",
                    [StorageServiceType.Queue] = "queueServices/default",
                    [StorageServiceType.File] = "fileServices/default",
                });

        /// <summary>
        /// Gets the resource ID for the specified resource, including the specified storage service type, if provided.
        /// The resource ID is a string in the ARM resource ID format, for example:
        /// <example>
        /// /subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Storage/storageAccounts/MyStorage/blobServices/default
        /// </example>
        /// </summary>
        /// <exception cref="ArgumentException">A specific storage service type was specified, but the specified resource is not of type <see cref="ResourceType.AzureStorage"/></exception>
        /// <param name="resourceIdentifier">The resource identifier</param>
        /// <param name="storageServiceType">The storage service type</param>
        /// <returns>The resource ID</returns>
        public static string ToResourceId(this ResourceIdentifier resourceIdentifier, StorageServiceType storageServiceType)
        {
            // Get the resource ID
            string resourceId = resourceIdentifier.ToResourceId();

            // Add a suffix that matches the storage service type
            if (storageServiceType != StorageServiceType.None)
            {
                // Verify that this is a storage resource
                if (resourceIdentifier.ResourceType != ResourceType.AzureStorage)
                {
                    throw new ArgumentException($"Unexpected resource type {resourceIdentifier.ResourceType}, expected type {ResourceType.AzureStorage}");
                }

                resourceId += "/" + MapStorageServiceTypeToString[storageServiceType];
            }

            return resourceId;
        }
    }
}