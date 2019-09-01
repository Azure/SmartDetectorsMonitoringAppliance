//-----------------------------------------------------------------------
// <copyright file="IAzureResourceManagerClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Arm
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An interface for fetching resource details from ARM.
    /// </summary>
    public interface IAzureResourceManagerClient
    {
        /// <summary>
        /// Retrieves the properties of the specified resource from ARM.
        /// The returned object contains the resource properties. Every ARM resource has different properties with
        /// different schema.
        /// For schema details for the properties of a specific ARM resource type:
        /// <code>https://docs.microsoft.com/azure/templates/{provider-namespace}/{resource-type}</code>
        /// For example, <a href="https://docs.microsoft.com/en-us/azure/templates/Microsoft.Compute/virtualMachines">this link</a>
        /// provides the schema for virtual machine properties.
        /// </summary>
        /// <param name="resource">The specific resource for which to retrieve the properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, and returning the resource properties.</returns>
        /// <exception cref="AzureResourceManagerClientException">Thrown when retrieving the resource properties from ARM fails.</exception>
        Task<ResourceProperties> GetResourcePropertiesAsync(ResourceIdentifier resource, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all the properties of the specified resource from ARM, using the specified ARM API version.
        /// The returned object contains the resource properties. Every ARM resource has different properties with
        /// different schema.
        /// For schema details for the properties of a specific ARM resource type:
        /// <code>https://docs.microsoft.com/azure/templates/{provider-namespace}/{resource-type}</code>
        /// For example, <a href="https://docs.microsoft.com/en-us/azure/templates/Microsoft.Compute/virtualMachines">this link</a>
        /// provides the schema for virtual machine properties.
        /// </summary>
        /// <param name="resource">The specific resource for which to retrieve the properties.</param>
        /// <param name="apiVersion">The ARM API version to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the current operation, and returning the resource properties.</returns>
        /// <exception cref="AzureResourceManagerClientException">Thrown when retrieving the resource properties from ARM fails.</exception>
        Task<ResourceProperties> GetResourcePropertiesAsync(ResourceIdentifier resource, string apiVersion, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resource groups in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource groups identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourceGroupsInSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources of the specified types in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInSubscriptionAsync(string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Enumerates all the resources in the specified resource group.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="resourceGroupName">The resource group name.</param>
        /// <param name="resourceTypes">The types of resource to enumerate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers.</returns>
        Task<IList<ResourceIdentifier>> GetAllResourcesInResourceGroupAsync(string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Executes an ARM GET request, using the specified resource, suffix, and query string.
        /// For example, the following call gets the list of databases for an SQL Server resource:
        /// <code>
        /// List&lt;JObject&gt; databases = await ExecuteArmQueryAsync(sqlResource, "/databases", "api-version=2017-10-01-preview", cancellationToken);
        /// </code>
        /// </summary>
        /// <param name="resource">The resource for the request.</param>
        /// <param name="suffix">The resource suffix.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>>A <see cref="Task{TResult}"/>, running the current operation, returning a list of all items returned.</returns>
        Task<List<JObject>> ExecuteArmQueryAsync(ResourceIdentifier resource, string suffix, string queryString, CancellationToken cancellationToken);

        /// <summary>
        /// Executes an ARM GET request, using the relative path, and query string.
        /// For example, the following call gets the list of databases for an SQL Server resource:
        /// <code>
        /// List&lt;JObject&gt; databases = await ExecuteArmQueryAsync("subscriptions/subscriptionId/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01", cancellationToken);
        /// </code>
        /// </summary>
        /// <param name="relativePath">The reltive path of the query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>>A <see cref="Task{TResult}"/>, running the current operation, returning a list of all items returned.</returns>
        Task<List<JObject>> ExecuteArmQueryAsync(Uri relativePath, CancellationToken cancellationToken);
    }
}