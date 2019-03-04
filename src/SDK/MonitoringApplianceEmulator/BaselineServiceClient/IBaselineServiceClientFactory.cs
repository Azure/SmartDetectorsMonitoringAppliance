//-----------------------------------------------------------------------
// <copyright file="IBaselineServiceClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for a factory of <see cref="IBaselineServiceClient"/> instances
    /// </summary>
    public interface IBaselineServiceClientFactory
    {
        /// <summary>
        /// Create a new instance of <see cref="IBaselineServiceClient"/>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{T}"/>, running the current operation, returning the baseline service client</returns>
        Task<IBaselineServiceClient> CreateAsync(CancellationToken cancellationToken);
    }
}