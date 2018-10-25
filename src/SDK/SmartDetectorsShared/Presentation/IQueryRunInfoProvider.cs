//-----------------------------------------------------------------------
// <copyright file="IQueryRunInfoProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// An interface for a provider of <see cref="QueryRunInfo"/> instances.
    /// </summary>
    public interface IQueryRunInfoProvider
    {
        /// <summary>
        /// Gets the run information to query telemetry for the the specified resources
        /// </summary>
        /// <param name="resources">The resources</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query run information</returns>
        Task<QueryRunInfo> GetQueryRunInfoAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken);
    }
}