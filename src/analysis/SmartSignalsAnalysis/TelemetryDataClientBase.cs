//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientBase.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
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
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.HttpClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Extensions;
    using Microsoft.Rest;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// A base class for implementations of <see cref="ITelemetryDataClient"/>.
    /// </summary>
    public abstract class TelemetryDataClientBase : ITelemetryDataClient
    {
        private readonly ITracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ServiceClientCredentials credentials;
        private readonly Uri queryUri;
        private readonly string dependencyName;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientBase"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="queryUri">The query URI</param>
        /// <param name="queryTimeout">The query timeout</param>
        /// <param name="dependencyName">The dependency name (for telemetry)</param>
        protected TelemetryDataClientBase(ITracer tracer, IHttpClientWrapper httpClientWrapper, ICredentialsFactory credentialsFactory, Uri queryUri, TimeSpan queryTimeout, string dependencyName)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.Timeout = Diagnostics.EnsureArgumentInRange(() => queryTimeout, TimeSpan.FromMinutes(0), TimeSpan.FromHours(2));
            this.queryUri = queryUri;
            this.dependencyName = dependencyName;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, dependencyName);

            // Extract the host part of the URI as the credentials resource
            UriBuilder builder = new UriBuilder()
            {
                Scheme = this.queryUri.Scheme,
                Host = this.queryUri.Host
            };

            Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            this.credentials = credentialsFactory.Create(builder.Uri.ToString());
        }

        /// <summary>
        /// Gets or sets the query timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Run a query against the relevant telemetry database. 
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        public async Task<IList<DataTable>> RunQueryAsync(string query, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Running query with an instance of {this.GetType().Name}");
            this.tracer.TraceVerbose($"Query: {query}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.queryUri);

            // Prepare request headers
            request.Headers.Add("Prefer", $"wait={this.Timeout.TotalSeconds}");

            // Prepare request content
            JObject requestContent = new JObject
            {
                ["query"] = query
            };

            this.UpdateRequestContent(requestContent);
            request.Content = new StringContent(requestContent.ToString(), Encoding.UTF8, "application/json");

            // Set the credentials
            if (this.credentials != null)
            {
                await this.credentials.ProcessHttpRequestAsync(request, cancellationToken);
            }

            // Send request and get the response as JSON
            Stopwatch queryStopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await this.retryPolicy.RunAndTrackDependencyAsync(this.tracer, this.dependencyName, "RunQuery", () => this.httpClientWrapper.SendAsync(request, cancellationToken));
            queryStopwatch.Stop();
            this.tracer.TraceInformation($"Query completed in {queryStopwatch.ElapsedMilliseconds}ms");
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject responseObject = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                // Parse the error and throw
                JObject errorObject = (JObject)responseObject["error"];
                this.tracer.TraceInformation($"Query returned an error: {errorObject}");
                throw new TelemetryDataClientException(errorObject, query);
            }
            else
            {
                IList<DataTable> dataTables = this.ReadTables(responseObject);
                this.tracer.TraceInformation($"Query returned {dataTables.Count} table{(dataTables.Count == 1 ? "" : "s")}, containing [{string.Join(",", dataTables.Select(dataTable => dataTable.Rows.Count.ToString()))}] rows");
                return dataTables;
            }
        }

        /// <summary>
        /// Update the HTTP request content with required values.
        /// </summary>
        /// <param name="requestContent">The request content.</param>
        protected abstract void UpdateRequestContent(JObject requestContent);

        /// <summary>
        /// Convert a Kusto type to a .Net type.
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
        /// Read the tables from the response object, and convert them to <see cref="DataTable"/> objects.
        /// </summary>
        /// <param name="responseObject">The response object.</param>
        /// <returns>The data tables.</returns>
        private IList<DataTable> ReadTables(JObject responseObject)
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
    }
}
