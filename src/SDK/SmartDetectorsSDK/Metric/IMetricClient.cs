//-----------------------------------------------------------------------
// <copyright file="IMetricClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Metric
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for fetching metrics for a specific azure resource.
    /// <see href="https://github.com/Azure-Samples/monitor-dotnet-metrics-api/blob/master/Program.cs">.NET Metric API example</see>
    /// <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-metrics-in-azure-monitor">Azure storage metrics example</see>
    /// <see href="https://docs.microsoft.com/en-us/powershell/module/azurerm.insights/get-azurermmetricdefinition?view=azurermps-5.4.0">Use Get-AzureRmMetricDefinition, to fetch for available metric names, granularity, etc.</see>
    /// </summary>
    public interface IMetricClient
    {
        /// <summary>
        /// Get the resource metric definitions
        /// </summary>
        /// <param name="resourceUri">The Uri path to the resource metrics API.
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list of metric definitions</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Keeping alignment with the Microsoft.Azure.Management.Monitor.Fluent.MetricDefinitionsOperationsExtensions API")]
        Task<IEnumerable<MetricDefinition>> GetResourceMetricDefinitionsAsync(string resourceUri, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric definitions, based on the resource and service (for example: if the resource is a storage account, possible services are BLOB, Queue, Table and File)
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metric definitions</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list of metric definitions</returns>
        Task<IEnumerable<MetricDefinition>> GetResourceMetricDefinitionsAsync(ResourceIdentifier resource, ServiceType azureResourceService, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric values
        /// </summary>
        /// <param name="resourceUri">The Uri to the resource metrics API.
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryProperties">Query properties to be used when fetching metric data. All fields are optional</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Keeping alignment with the Microsoft.Azure.Management.Monitor.Fluent.MetricDefinitionsOperationsExtensions API")]
        Task<IEnumerable<MetricQueryResult>> GetResourceMetricsAsync(string resourceUri, QueryParameters queryProperties, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric values, based on the resource and service (for example: if the resource is a storage account, possible services are BLOB, Queue, Table and File)
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metrics</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <param name="queryProperties">Query properties to be used when fetching metric data. All fields are optional</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        Task<IEnumerable<MetricQueryResult>> GetResourceMetricsAsync(ResourceIdentifier resource, ServiceType azureResourceService, QueryParameters queryProperties, CancellationToken cancellationToken);
    }
}