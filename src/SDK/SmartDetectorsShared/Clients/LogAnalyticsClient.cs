//-----------------------------------------------------------------------
// <copyright file="LogAnalyticsClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Rest;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// A base class for implementations of <see cref="ILogAnalyticsClient"/>.
    /// </summary>
    public class LogAnalyticsClient : ILogAnalyticsClient
    {
        private const string LogAnalyticsQueryAppName = "SmartDetectorsMonitoringAppliance";

        private readonly ITracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ICredentialsFactory credentialsFactory;
        private readonly Policy<HttpResponseMessage> retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAnalyticsClient"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="queryUri">The URI to use for querying telemetry - this should already include the resource to query</param>
        /// <param name="queryTimeout">The query timeout</param>
        public LogAnalyticsClient(
            ITracer tracer,
            IHttpClientWrapper httpClientWrapper,
            ICredentialsFactory credentialsFactory,
            Uri queryUri,
            TimeSpan queryTimeout)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.credentialsFactory = Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            this.QueryUri = Diagnostics.EnsureArgumentNotNull(() => queryUri);
            this.Timeout = Diagnostics.EnsureArgumentInRange(() => queryTimeout, TimeSpan.FromMinutes(0), TimeSpan.FromHours(2));

            this.retryPolicy = PolicyExtensions.CreateTransientHttpErrorPolicy(this.tracer, "LogAnalytics");
        }

        /// <summary>
        /// Gets the query URI (used as a test-hook for validations).
        /// </summary>
        public Uri QueryUri { get; }

        /// <summary>
        /// Gets or sets the query timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Run a query with a specific timespan against the relevant telemetry database.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="dataTimeSpan">A time span to use for limiting the query data range.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        public async Task<IList<DataTable>> RunQueryAsync(string query, TimeSpan dataTimeSpan, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Running query with an instance of {this.GetType().Name}");
            this.tracer.TraceVerbose($"Query: {query}");

            HttpResponseMessage response = await this.InternalRunQueryAsync(query, dataTimeSpan, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new TelemetryDataClientException(this.ParseErrorMessage(await response.Content.ReadAsStringAsync()));
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            JObject responseObject = JObject.Parse(responseContent);
            IList<DataTable> dataTables = ReadTables(responseObject);
            this.tracer.TraceInformation($"Query returned {dataTables.Count} table{(dataTables.Count == 1 ? string.Empty : "s")}, containing [{string.Join(",", dataTables.Select(dataTable => dataTable.Rows.Count))}] rows");

            return dataTables;
        }

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
        /// Parses an error messgae from HTTP response content to JSon object
        /// </summary>
        /// <param name="responseContent">The response content.</param>
        /// <returns>The JSon object representation of an error.</returns>
        private string ParseErrorMessage(string responseContent)
        {
            try
            {
                JObject responseObject = JObject.Parse(responseContent);
                JObject errorObject = (JObject)responseObject["error"];
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
        /// <param name="dataTimeSpan">A time span to use for limiting the query data range.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning HTTP response for the request.</returns>
        private async Task<HttpResponseMessage> InternalRunQueryAsync(string query, TimeSpan dataTimeSpan, CancellationToken cancellationToken)
        {
            // Extract the host part of the URI as the credentials resource
            UriBuilder builder = new UriBuilder()
            {
                Scheme = this.QueryUri.Scheme,
                Host = this.QueryUri.Host
            };

            ServiceClientCredentials credentials = this.credentialsFactory.CreateServiceClientCredentials(builder.Uri.ToString());

            string requestId = Guid.NewGuid().ToString();

            // Send the request
            Stopwatch queryStopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                "LogAnalytics",
                "RunQuery",
                async () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.QueryUri);

                    // Prepare request headers
                    request.Headers.Add("Prefer", $"wait={this.Timeout.TotalSeconds}, ai.include-error-payload=true");
                    request.Headers.Add("x-ms-app", LogAnalyticsQueryAppName);
                    request.Headers.Add("x-ms-client-request-id", requestId);

                    // Prepare request content
                    var requestContent = new JObject
                    {
                        ["query"] = query,
                        ["timespan"] = System.Xml.XmlConvert.ToString(dataTimeSpan)
                    };
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
                    ["QueryUri"] = this.QueryUri.ToString(),
                });

            queryStopwatch.Stop();
            this.tracer.TraceInformation($"Query completed in {queryStopwatch.ElapsedMilliseconds}ms, request Id {requestId}");
            return response;
        }
    }
}
