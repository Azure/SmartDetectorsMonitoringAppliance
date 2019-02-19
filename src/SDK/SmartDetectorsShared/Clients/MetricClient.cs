//-----------------------------------------------------------------------
// <copyright file="MetricClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Polly;
    using MetricDefinition = Microsoft.Azure.Monitoring.SmartDetectors.Metric.MetricDefinition;

    /// <summary>
    /// A metric client that implements <see cref="IMetricClient"/>.
    /// </summary>
    public class MetricClient : IMetricClient
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "Metric";

        private readonly IExtendedTracer tracer;
        private readonly Policy retryPolicy;

        private readonly IMonitorManagementClient monitorManagementClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricClient"/> class
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="monitorManagementClient">Monitor management client to use to fetch metric data</param>
        public MetricClient(IExtendedTracer tracer, IMonitorManagementClient monitorManagementClient)
        {
            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            if (monitorManagementClient == null)
            {
                throw new ArgumentNullException(nameof(monitorManagementClient));
            }

            this.tracer = tracer;
            this.monitorManagementClient = monitorManagementClient;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricClient"/> class
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        public MetricClient(IExtendedTracer tracer, ICredentialsFactory credentialsFactory)
            : this(tracer, new MonitorManagementClient(RestClient.Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithCredentials(credentialsFactory.CreateAzureCredentials("https://management.azure.com/")).Build()))
        {
        }

        /// <summary>
        /// Get the resource metric definitions
        /// </summary>
        /// <param name="resourceUri">The Uri to the resource metrics API.
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list of metric definitions</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Keeping alignment with the Microsoft.Azure.Management.Monitor.Fluent.MetricDefinitionsOperationsExtensions API")]
        public async Task<IEnumerable<MetricDefinition>> GetResourceMetricDefinitionsAsync(string resourceUri, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Running GetResourceMetricDefinitions with an instance of {this.GetType().Name}");
            List<Management.Monitor.Fluent.Models.MetricDefinition> definitions = (await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                "GetResourceMetricDefinitions",
                () => this.monitorManagementClient.MetricDefinitions.ListAsync(
                    resourceUri: resourceUri,
                    cancellationToken: cancellationToken))).ToList();

            this.tracer.TraceInformation($"Running GetResourceMetricDefinitions completed. Total Definitions: {definitions.Count}.");
            return definitions.Select(definition => definition.ToSmartDetectorsMetricDefinition());
        }

        /// <summary>
        /// Get the resource metric definitions, based on the resource and service (for example: if the resource is a storage account, possible services are BLOB, Queue, Table and File)
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metric definitions</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list of metric definitions</returns>
        public async Task<IEnumerable<MetricDefinition>> GetResourceMetricDefinitionsAsync(ResourceIdentifier resource, ServiceType azureResourceService, CancellationToken cancellationToken)
        {
            string resourceFullUri = resource.GetResourceFullUri(azureResourceService);
            return await this.GetResourceMetricDefinitionsAsync(resourceFullUri, cancellationToken);
        }

        /// <summary>
        /// Get the resource metric values
        /// </summary>
        /// <param name="resourceUri">The Uri to the resource metrics API.
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryParameters">Query properties to be used when fetching metric data. All fields are optional</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Keeping alignment with the Microsoft.Azure.Management.Monitor.Fluent.MetricDefinitionsOperationsExtensions API")]
        public async Task<IEnumerable<MetricQueryResult>> GetResourceMetricsAsync(string resourceUri, QueryParameters queryParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.tracer.TraceInformation($"Running GetResourceMetrics with an instance of {this.GetType().Name}, with params: {queryParameters}");
            ResponseInner metrics = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                "GetResourceMetrics",
                () => this.monitorManagementClient.Metrics.ListAsync(
                    resourceUri: resourceUri,
                    timespan: queryParameters.TimeRange,
                    interval: queryParameters.Interval,
                    metricnames: queryParameters.MetricNames == null ? string.Empty : string.Join(",", queryParameters.MetricNames),
                    aggregation: queryParameters.Aggregations != null ? string.Join(",", queryParameters.Aggregations) : null,
                    top: queryParameters.Top,
                    orderby: queryParameters.Orderby,
                    odataQuery: queryParameters.Filter,
                    resultType: null,
                    cancellationToken: cancellationToken));

            this.tracer.TraceInformation($"Running GetResourceMetrics completed. Total Metrics: {metrics.Value.Count}.");
            IList<MetricQueryResult> result = metrics.ToMetricQueryResult();

            return result;
        }

        /// <summary>
        /// Get the resource metric values, based on the resource and service (for example: if the resource is a storage account, possible services are BLOB, Queue, Table and File)
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metrics</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <param name="queryParameters">Query properties to be used when fetching metric data. All fields are optional</param>
        /// <param name="cancellationToken">Cancellation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        public async Task<IEnumerable<MetricQueryResult>> GetResourceMetricsAsync(ResourceIdentifier resource, ServiceType azureResourceService, QueryParameters queryParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            string resourceFullUri = resource.GetResourceFullUri(azureResourceService);
            return await this.GetResourceMetricsAsync(resourceFullUri, queryParameters, cancellationToken);
        }
    }
}