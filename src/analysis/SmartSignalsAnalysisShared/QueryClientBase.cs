//-----------------------------------------------------------------------
// <copyright file="QueryClientBase.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A base class for implementations of <see cref="IQueryClient"/>.
    /// </summary>
    public abstract class QueryClientBase : IQueryClient
    {
        private readonly IHttpClientWrapper httpClientWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryClientBase"/> class.
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="queryTimeout">The query timeout.</param>
        protected QueryClientBase(IHttpClientWrapper httpClientWrapper, TimeSpan queryTimeout)
        {
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.Timeout = Diagnostics.EnsureArgumentInRange(() => queryTimeout, TimeSpan.FromMinutes(0), TimeSpan.FromHours(2));
        }

        /// <summary>
        /// Gets or sets the query timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets the URI for REST API calls
        /// </summary>
        protected abstract Uri QueryUri { get; }

        /// <summary>
        /// Run a query against the relevant telemetry database. 
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        public async Task<IList<DataTable>> RunQueryAsync(string query, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.QueryUri);

            // Prepare request headers
            request.Headers.Add("Prefer", $"wait={this.Timeout.TotalSeconds}");

            // Prepare request content
            JObject requestContent = new JObject
            {
                ["query"] = query
            };

            this.UpdateRequestContent(requestContent);
            request.Content = new StringContent(requestContent.ToString(), Encoding.UTF8, "application/json");
            
            // Send request and get the response as JSON
            HttpResponseMessage response = await this.httpClientWrapper.SendAsync(request, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject responseObject = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                // Parse the error and throw
                JObject errorObject = (JObject)responseObject["error"];
                throw new QueryClientException(errorObject, query);
            }
            else
            {
                return this.ReadTables(responseObject);
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
