//-----------------------------------------------------------------------
// <copyright file="ResourceIdentifierExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;

    /// <summary>
    /// Extension methods for <see cref="ResourceIdentifier"/>
    /// </summary>
    public static class ResourceIdentifierExtensions
    {
        /// <summary>
        /// A dictionary, mapping <see cref="ServiceType"/> enumeration values to matching presentation in URI
        /// </summary>
        private static readonly ReadOnlyDictionary<ServiceType, string> MapAzureServiceTypeToPresentationInUri =
            new ReadOnlyDictionary<ServiceType, string>(
                new Dictionary<ServiceType, string>()
                {
                    [ServiceType.AzureStorageBlob] = "blobServices/default",
                    [ServiceType.AzureStorageTable] = "tableServices/default",
                    [ServiceType.AzureStorageQueue] = "queueServices/default",
                    [ServiceType.AzureStorageFile] = "fileServices/default",
                });

        /// <summary>
        /// Builds the full Resource metrics Uri based on <see cref="ServiceType"/>.
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metrics</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <returns>The full Resource metrics Uri</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Keeping alignment with the Microsoft.Azure.Management nugets APIs")]
        public static string GetResourceFullUri(this ResourceIdentifier resource, ServiceType azureResourceService)
        {
            string uri = resource.ToResourceId();
            if (azureResourceService != ServiceType.None)
            {
                uri += "/" + MapAzureServiceTypeToPresentationInUri[azureResourceService];
            }

            return uri;
        }
    }
}
