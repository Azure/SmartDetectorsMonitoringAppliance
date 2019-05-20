//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// A base class for implementations of <see cref="ITelemetryDataClient"/>.
    /// </summary>
    public abstract class TelemetryDataClientBase : ITelemetryDataClient
    {
        private const int TelemetryResourcesInDraftRequestLimit = 15;
        private const string DraftAppName = "SmartAlertsRuntime";

        private readonly ITracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ICredentialsFactory credentialsFactory;
        private readonly string queryUriFormat;
        private readonly Policy<HttpResponseMessage> retryPolicy;
        private readonly string telemetryDbType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientBase"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="queryUriFormat">The query URI format</param>
        /// <param name="queryTimeout">The query timeout</param>
        /// <param name="telemetryDbType">The type of telemetry DB that this data client accesses</param>
        /// <param name="telemetryResourceIds">the telemetry resource IDs - the IDs of the resources that store the telemetry that this data client accesses</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "The parameter is not a URI, but a format string for creating URIs")]
        protected TelemetryDataClientBase(
            ITracer tracer,
            IHttpClientWrapper httpClientWrapper,
            ICredentialsFactory credentialsFactory,
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            string queryUriFormat,
            TimeSpan queryTimeout,
            string telemetryDbType,
            IEnumerable<string> telemetryResourceIds)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.credentialsFactory = Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            this.AzureResourceManagerClient = Diagnostics.EnsureArgumentNotNull(() => azureResourceManagerClient);
            this.queryUriFormat = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => queryUriFormat);
            this.Timeout = Diagnostics.EnsureArgumentInRange(() => queryTimeout, TimeSpan.FromMinutes(0), TimeSpan.FromHours(2));
            this.telemetryDbType = telemetryDbType;

            int maximumNumberOfTelemetryResources = int.Parse(ConfigurationManager.AppSettings["MaximumNumberOfTelemetryResources"] ?? "300", CultureInfo.InvariantCulture);
            this.TelemetryResourceIds = telemetryResourceIds?.Take(maximumNumberOfTelemetryResources).ToList() ?? new List<string>();

            this.retryPolicy = PolicyExtensions.CreateTransientHttpErrorPolicy(this.tracer, this.telemetryDbType);
        }

        /// <summary>
        /// Gets or sets the query timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets the telemetry resource IDs - the IDs of the resources
        /// that store the telemetry that this data client accesses.
        /// </summary>
        public IReadOnlyList<string> TelemetryResourceIds { get; }

        /// <summary>
        /// Gets the Azure Resource Manager client.
        /// </summary>
        protected IExtendedAzureResourceManagerClient AzureResourceManagerClient { get; }

        /// <summary>
        /// Gets the key in the Draft request for additional telemetry resource IDs
        /// </summary>
        protected abstract string AdditionalTelemetryResourceIdsRequestKey { get; }

        /// <summary>
        /// Runs a query against the relevant telemetry database.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        public async Task<IList<DataTable>> RunQueryAsync(string query, CancellationToken cancellationToken)
        {
            return await this.RunQueryAsync(query, null, cancellationToken);
        }

        /// <summary>
        /// Run a query with a specific timespan against the relevant telemetry database.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="dataTimeSpan">
        /// An optional time span to use for limiting the query data range. If this contains <c>null</c> then no limitation will be applied.
        /// </param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        public async Task<IList<DataTable>> RunQueryAsync(string query, TimeSpan? dataTimeSpan, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Running query with an instance of {this.GetType().Name}");
            this.tracer.TraceVerbose($"Query: {query}");

            // Calculate the number of iterations based on maximum allowed number of telemetry resources that can be queried at once
            int numberOfIterations = (int)Math.Ceiling(this.TelemetryResourceIds.Count / (1.0 * TelemetryResourcesInDraftRequestLimit));

            // Iterate on telemetry resources in batches, combining results into a single data table
            DataTable resultTable = null;
            for (int i = 0; i < numberOfIterations; i++)
            {
                var iterationTelemetryResourcesIds = this.TelemetryResourceIds.Skip(i * TelemetryResourcesInDraftRequestLimit).Take(TelemetryResourcesInDraftRequestLimit).ToList();

                // "Telemetry id" is a GUID which is either Application Insights application id or Log Analytics workspace id.
                // Not to be confused with "telemetry resource id" which is an identifier of an application or a workspace as an Azure resource,
                // for example: "subscriptions/17fe03d9-f83f-44ac-abc9-dc19df3ee8ec/resourcegroups/someResourceGroup/providers/microsoft.operationalinsights/workspaces/someWorkspace"
                List<string> additionalTelemetryResourceIds = new List<string>(iterationTelemetryResourcesIds);
                string telemetryId = await this.GetSingleTelemetryIdAsync(additionalTelemetryResourceIds, cancellationToken);
                if (telemetryId == null)
                {
                    // If this is the last iteration of many - skip this iteration.
                    // Last iteration may contain only one or two telemetry resources.
                    // So the chances that all the resources in the last iteration will turn out to be deleted are higher than in other iterations.
                    // We don't want a single deleted resource at the end to cause entire query execution to be aborted.
                    if (i == numberOfIterations - 1 && numberOfIterations > 1)
                    {
                        continue;
                    }
                    else
                    {
                        throw new TelemetryDataClientException("None of supplied telemetry resources were found");
                    }
                }

                HttpResponseMessage response = await this.InternalRunQueryAsync(query, additionalTelemetryResourceIds, telemetryId, dataTimeSpan, cancellationToken);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // One reason for bad status response may be that one or more telemetry resources were deleted.
                    // Because of ARM caching we may get deleted resources when listing all resources in a subscription.

                    // Filter deleted resources
                    var filteredAdditionalTelemetryResourceIds = await this.FilterDeletedTelemetryResourcesAsync(additionalTelemetryResourceIds, cancellationToken);

                    // If number of telemetry resources didn't change after filtration - deleted resources were not the reason for bad request status.
                    // Throw an exception with the original response content
                    if (filteredAdditionalTelemetryResourceIds.Count == additionalTelemetryResourceIds.Count)
                    {
                        throw new TelemetryDataClientException(this.ParseErrorMessage(await response.Content.ReadAsStringAsync()));
                    }

                    // Try again with filtered resources
                    response = await this.InternalRunQueryAsync(query, filteredAdditionalTelemetryResourceIds, telemetryId, dataTimeSpan, cancellationToken);
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new TelemetryDataClientException(this.ParseErrorMessage(await response.Content.ReadAsStringAsync()));
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                JObject responseObject = JObject.Parse(responseContent);
                IList<DataTable> dataTables = ReadTables(responseObject);
                this.tracer.TraceInformation($"Iteration {i} out of {numberOfIterations}: query returned {dataTables.Count} table{(dataTables.Count == 1 ? string.Empty : "s")}, containing [{string.Join(",", dataTables.Select(dataTable => dataTable.Rows.Count))}] rows");

                // Merge main data table with returned data table
                if (dataTables.Count > 0)
                {
                    if (resultTable == null)
                    {
                        resultTable = dataTables[0];
                    }
                    else
                    {
                        resultTable.Merge(dataTables[0]);
                    }
                }
            }

            this.tracer.TraceInformation($"Query returned {resultTable?.Rows.Count ?? 0} rows in total");
            return resultTable == null ? new List<DataTable>() : new List<DataTable> { resultTable };
        }

        /// <summary>
        /// Gets the id of the telemetry resource
        /// </summary>
        /// <param name="telemetryResource">The telemetry resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, running the telemetry resource id.</returns>
        protected abstract Task<string> GetTelemetryResourceIdAsync(ResourceIdentifier telemetryResource, CancellationToken cancellationToken);

        /// <summary>
        /// Converts a Kusto type to a .Net type.
        /// </summary>
        /// <param name="kustoColumnType">The Kusto type.</param>
        /// <returns>The .Net type.</returns>
        private static Type ConvertType(string kustoColumnType)
        {
            switch (kustoColumnType)
            {
                case "bool":
                    return typeof(bool);
                case "datetime":
                    return typeof(DateTime);
                case "dynamic":
                    return typeof(object);
                case "guid":
                    return typeof(Guid);
                case "int":
                    return typeof(int);
                case "long":
                    return typeof(long);
                case "real":
                    return typeof(double);
                case "string":
                    return typeof(string);
                case "timespan":
                    return typeof(TimeSpan);
                default:
                    throw new ArgumentException($"Unsupported Kusto type: {kustoColumnType}");
            }
        }

        /// <summary>
        /// Reads the tables from the response object, and convert them to <see cref="DataTable"/> objects.
        /// </summary>
        /// <param name="responseObject">The response object.</param>
        /// <returns>The data tables.</returns>
        private static IList<DataTable> ReadTables(JObject responseObject)
        {
            // Parse the results to a list of DataTable objects
            List<DataTable> dataTables = new List<DataTable>();
            JArray tables = (JArray)responseObject["tables"];
            foreach (JObject table in tables.OfType<JObject>())
            {
                // Create table and set table name
                DataTable dataTable = new DataTable()
                {
                    TableName = table["name"]?.ToObject<string>() ?? string.Empty
                };

                // Read table columns
                JArray columns = (JArray)table["columns"];
                foreach (JObject column in columns.OfType<JObject>())
                {
                    dataTable.Columns.Add(column["name"].ToString(), ConvertType(column["type"].ToString()));
                }

                // Read table rows
                JArray rows = (JArray)table["rows"];
                foreach (JArray row in rows.OfType<JArray>())
                {
                    object[] values = new object[dataTable.Columns.Count];
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        values[i] = row[i].Type == JTokenType.Null ? DBNull.Value : row[i].ToObject(dataTable.Columns[i].DataType);
                    }

                    dataTable.Rows.Add(values);
                }

                dataTables.Add(dataTable);
            }

            return dataTables;
        }

        /// <summary>
        /// Returns a filtered subset of supplied list of resources, containing only currently existing resources
        /// </summary>
        /// <param name="telemetryResourceIds">Telemetry resources to filter</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning filtered list of existing resources.</returns>
        private async Task<List<string>> FilterDeletedTelemetryResourcesAsync(IReadOnlyList<string> telemetryResourceIds, CancellationToken cancellationToken)
        {
            var filteredTelemetryResourceIds = new List<string>();
            foreach (var telemetryResourceId in telemetryResourceIds)
            {
                try
                {
                    await this.AzureResourceManagerClient.GetResourcePropertiesAsync(ResourceIdentifier.CreateFromResourceId(telemetryResourceId), cancellationToken);
                }
                catch (AzureResourceManagerClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    this.tracer.TraceWarning($"Telemetry resource '{telemetryResourceId}' is not found, skipping this resource");
                    continue;
                }

                filteredTelemetryResourceIds.Add(telemetryResourceId);
            }

            this.tracer.TraceInformation($"{filteredTelemetryResourceIds.Count} out of {telemetryResourceIds.Count} telemetry resources left after filtration");

            return filteredTelemetryResourceIds;
        }

        /// <summary>
        /// Returns telemetry id of the first resource in the supplied telemetry resources list,
        /// and removes that first resource from the list.
        /// If the first resource doesn't exist, the method will remove it from the list and continue
        /// until an existing resource is found (in which case the method will return that resource's
        /// telemetry id, and will remove that resource from the list).
        /// </summary>
        /// <param name="telemetryResourceIds">Telemetry resources to filter</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning telemetry id or null if none of supplied telemetry resources were found</returns>
        private async Task<string> GetSingleTelemetryIdAsync(List<string> telemetryResourceIds, CancellationToken cancellationToken)
        {
            string telemetryId = null;
            while (telemetryId == null && telemetryResourceIds.Count > 0)
            {
                string candidateTelemetryResourceId = null;
                try
                {
                    candidateTelemetryResourceId = telemetryResourceIds.First();
                    telemetryId = await this.GetTelemetryResourceIdAsync(ResourceIdentifier.CreateFromResourceId(candidateTelemetryResourceId), cancellationToken);
                }
                catch (AzureResourceManagerClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    this.tracer.TraceWarning($"Telemetry resource '{candidateTelemetryResourceId}' is not found{(telemetryResourceIds.Count > 1 ? ", will attempt to get telemetry id from another resource" : string.Empty)}");
                }

                telemetryResourceIds.Remove(candidateTelemetryResourceId);
            }

            return telemetryId;
        }

        /// <summary>
        /// Parses an error messgae from HTTP response content to JSon object
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <returns>The JSon object representation of an error.</returns>
        private string ParseErrorMessage(string responseContent)
        {
            JObject errorObject = null;
            try
            {
                JObject responseObject = JObject.Parse(responseContent);
                errorObject = (JObject)responseObject["error"];
                this.tracer.TraceError($"Query returned an error: {errorObject}");

                return $"[{errorObject["code"]}] {errorObject["message"]}";
            }
            catch (Exception ex)
            {
                // Don't throw in error handling
                this.tracer.TraceError($"Query returned an error. In addition, error details parsing failed with exception: {ex}");
                return "Unspecified error";
            }
        }

        /// <summary>
        /// Runs the query against supplied telemetry resources
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="additionalTelemetryResourceIds">The additional telemetry resources on which to run the query (other than the specified single telemetry Id).</param>
        /// <param name="singleTelemetryId">The telemetry id of a single telemetry resource.</param>
        /// <param name="dataTimeSpan">
        /// An optional time span to use for limiting the query data range. If this contains <c>null</c> then no limitation will be applied.
        /// </param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning HTTP response for the request.</returns>
        private async Task<HttpResponseMessage> InternalRunQueryAsync(string query, IReadOnlyList<string> additionalTelemetryResourceIds, string singleTelemetryId, TimeSpan? dataTimeSpan, CancellationToken cancellationToken)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, this.queryUriFormat, singleTelemetryId));

            // Extract the host part of the URI as the credentials resource
            UriBuilder builder = new UriBuilder()
            {
                Scheme = uri.Scheme,
                Host = uri.Host
            };

            var credentials = this.credentialsFactory.CreateServiceClientCredentials(builder.Uri.ToString());

            string requestId = Guid.NewGuid().ToString();

            // Send the request
            Stopwatch queryStopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                this.telemetryDbType,
                "RunQuery",
                async () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

                    // Prepare request headers
                    request.Headers.Add("Prefer", $"wait={this.Timeout.TotalSeconds}, ai.include-error-payload=true");
                    request.Headers.Add("x-ms-app", DraftAppName);
                    request.Headers.Add("x-ms-client-request-id", requestId);

                    // Prepare request content
                    JObject requestContent = new JObject
                    {
                        ["query"] = query
                    };

                    // Add timespan if it was specified
                    if (dataTimeSpan != null)
                    {
                        requestContent["timespan"] = System.Xml.XmlConvert.ToString(dataTimeSpan.Value);
                    }

                    // Add additional telemetry resources if needed
                    if (additionalTelemetryResourceIds.Count > 0)
                    {
                        requestContent[this.AdditionalTelemetryResourceIdsRequestKey] = new JArray(additionalTelemetryResourceIds);
                    }

                    request.Content = new StringContent(requestContent.ToString(), Encoding.UTF8, "application/json");

                    // Set the credentials
                    if (credentials != null)
                    {
                        await credentials.ProcessHttpRequestAsync(request, cancellationToken);
                    }

                    return await this.httpClientWrapper.SendAsync(request, this.Timeout, cancellationToken);
                },
                properties: new Dictionary<string, string>
                {
                    ["DraftRequestId"] = requestId,
                    ["MainResourceId"] = singleTelemetryId,
                });

            queryStopwatch.Stop();
            this.tracer.TraceInformation($"Query completed in {queryStopwatch.ElapsedMilliseconds}ms, request Id {requestId}");
            return response;
        }
    }
}
