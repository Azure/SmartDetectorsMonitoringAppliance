//-----------------------------------------------------------------------
// <copyright file="IActivityLogClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An interface for fetching resource activity from Activity Log REST API.
    /// </summary>
    public interface IActivityLogClient
    {
        /// <summary>
        /// Retrieves the Activity Log entries for <paramref name="resourceIdentifier"/> in the specified time range.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier to get the Activity Log entries for</param>
        /// <param name="startTime">The start time of the period to query Activity Log entries for</param>
        /// <param name="endTime">The end time of the period to query Activity Log entries for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete</param>
        /// <returns>A <see cref="Task"/>, returning the Activity Log entries as a list of <see cref="JObject"/></returns>
        Task<List<JObject>> GetActivityLogAsync(ResourceIdentifier resourceIdentifier, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
    }
}
