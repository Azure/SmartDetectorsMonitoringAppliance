//-----------------------------------------------------------------------
// <copyright file="MetricClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Polly;
    using MetricDefinition = Microsoft.Azure.Monitoring.SmartDetectors.Metric.MetricDefinition;

    /// <summary>
    /// A metric client that implements <see cref="IMetricClient"/>.
    /// </summary>
    public class MetricClient : IMetricClient
    {
        /// <summary>
        /// A dictionary, mapping <see cref="ServiceType"/> enumeration values to matching presentation in URI
        /// </summary>
        public static readonly ReadOnlyDictionary<ServiceType, string> MapAzureServiceTypeToPresentationInUri =
            new ReadOnlyDictionary<ServiceType, string>(
                new Dictionary<ServiceType, string>()
                {
                    [ServiceType.AzureStorageBlob] = "blobServices/default",
                    [ServiceType.AzureStorageTable] = "tableServices/default",
                    [ServiceType.AzureStorageQueue] = "queueServices/default",
                    [ServiceType.AzureStorageFile] = "fileServices/default",
                });

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
        /// <param name="subscriptionId">The subscription Id</param>
        /// <param name="monitorManagementClient">Monitor management client to use to fetch metric data</param>
        public MetricClient(IExtendedTracer tracer, string subscriptionId, IMonitorManagementClient monitorManagementClient)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            this.monitorManagementClient = monitorManagementClient;
            this.monitorManagementClient.SubscriptionId = subscriptionId;
            this.tracer = tracer;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricClient"/> class
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="subscriptionId">The subscription Id</param>
        public MetricClient(IExtendedTracer tracer, ICredentialsFactory credentialsFactory, string subscriptionId)
            : this(tracer, subscriptionId, new MonitorManagementClient(credentialsFactory.Create("https://management.azure.com/")))
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
            return definitions.Select(ConvertMetricDefinition);
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
            string resourceFullUri = GetResourceFullUri(resource, azureResourceService);
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
            IList<MetricQueryResult> result = this.ConvertResponseToQueryResult(metrics);

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
            string resourceFullUri = GetResourceFullUri(resource, azureResourceService);
            return await this.GetResourceMetricsAsync(resourceFullUri, queryParameters, cancellationToken);
        }

        /// <summary>
        /// Converts a metric definition response to an internal DTO and returns it
        /// </summary>
        /// <param name="definition">The metric definition</param>
        /// <returns>The conversion result</returns>
        private static MetricDefinition ConvertMetricDefinition(Management.Monitor.Fluent.Models.MetricDefinition definition)
        {
            return new MetricDefinition(
                definition.Name.Value,
                definition.Dimensions?.Select(x => x.Value).ToList(),
                definition.IsDimensionRequired,
                definition.MetricAvailabilities?.Select(x => Tuple.Create(x.Retention, x.TimeGrain)).ToList(),
                definition.Unit?.ToString(),
                definition.PrimaryAggregationType.HasValue ? (Aggregation?)Enum.Parse(typeof(Aggregation), definition.PrimaryAggregationType.ToString()) : null);
        }

        /// <summary>
        /// Builds the full Resource metrics Uri based on <see cref="ServiceType"/>.
        /// </summary>
        /// <param name="resource">The Azure resource for which we want to fetch metrics</param>
        /// <param name="azureResourceService">The Azure resource's service type</param>
        /// <returns>The full Resource metrics Uri</returns>
        private static string GetResourceFullUri(ResourceIdentifier resource, ServiceType azureResourceService)
        {
            string uri = resource.ToResourceId();
            if (azureResourceService != ServiceType.None)
            {
                uri += "/" + MapAzureServiceTypeToPresentationInUri[azureResourceService];
            }

            return uri;
        }

        /// <summary>
        /// Converts a metric query response to an internal DTO and returns it
        /// </summary>
        /// <param name="queryResponse">The metric query response as returned by Azure Monitoring</param>
        /// <returns>A list of metric query results</returns>
        private IList<MetricQueryResult> ConvertResponseToQueryResult(ResponseInner queryResponse)
        {
            var queryResults = new List<MetricQueryResult>();

            // Convert each metric (a single metric is created per metric name)
            foreach (Metric metric in queryResponse.Value)
            {
                List<MetricTimeSeries> timeSeriesList = new List<MetricTimeSeries>();

                if (metric.Timeseries != null)
                {
                    // Convert the time series. A time series is created per filtered dimension.
                    // The info regarding the relevant dimension is set int he MetaData field
                    foreach (TimeSeriesElement timeSeries in metric.Timeseries)
                    {
                        var data = new List<MetricValues>();
                        var metaData = new List<KeyValuePair<string, string>>();

                        if (timeSeries.Data != null)
                        {
                            // Convert all metric values
                            data = timeSeries.Data.Select(metricValue =>
                                new MetricValues(metricValue.TimeStamp, metricValue.Average, metricValue.Minimum, metricValue.Maximum, metricValue.Total, metricValue.Count)).ToList();
                        }

                        if (timeSeries.Metadatavalues != null)
                        {
                            // Convert metadata
                            metaData = timeSeries.Metadatavalues.Select(metaDataValue =>
                                new KeyValuePair<string, string>(metaDataValue.Name.Value, metaDataValue.Value)).ToList();
                        }

                        timeSeriesList.Add(new MetricTimeSeries(data, metaData));
                    }
                }

                var queryResult = new MetricQueryResult(metric.Name.Value, metric.Unit.ToString(), timeSeriesList);

                queryResults.Add(queryResult);
                this.tracer.TraceInformation($"Metric converted successfully. Name: {queryResult.Name}, Timeseries count: {queryResult.Timeseries.Count}, Total series length: {queryResult.Timeseries.Sum(timeSeries => timeSeries.Data.Count)}");
            }

            return queryResults;
        }
   }
}