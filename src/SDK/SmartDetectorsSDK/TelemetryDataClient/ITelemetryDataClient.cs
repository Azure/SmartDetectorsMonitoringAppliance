//-----------------------------------------------------------------------
// <copyright file="ITelemetryDataClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for running queries against the relevant telemetry database.
    /// </summary>
    public interface ITelemetryDataClient
    {
        /// <summary>
        /// Gets or sets the query timeout.
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Run a query against the relevant telemetry database.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        Task<IList<DataTable>> RunQueryAsync(string query, CancellationToken cancellationToken);

        /// <summary>
        /// Run a query with a specific timespan against the relevant telemetry database.
        /// </summary>
        /// <param name="query">The query to run.</param>
        /// <param name="dataTimeSpan">
        /// An optional time span to use for limiting the query data range. If this contains <c>null</c> then no limitation will be applied.
        /// </param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query result.</returns>
        Task<IList<DataTable>> RunQueryAsync(string query, TimeSpan? dataTimeSpan, CancellationToken cancellationToken);
    }
}
