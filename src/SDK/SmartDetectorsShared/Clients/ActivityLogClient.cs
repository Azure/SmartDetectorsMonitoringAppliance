//-----------------------------------------------------------------------
// <copyright file="ActivityLogClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.ActivityLog;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Newtonsoft.Json.Linq;
    using Polly;

    /// <summary>
    /// An implementation of <see cref="IActivityLogClient"/> to access Activity Log REST API.
    /// </summary>
    public class ActivityLogClient : IActivityLogClient
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "ARM";

        private readonly IExtendedTracer tracer;
        private readonly Policy retryPolicy;
        private readonly IAzureResourceManagerClient azureResourceManagerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogClient"/> class.
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="tracer">The tracer</param>
        public ActivityLogClient(IAzureResourceManagerClient azureResourceManagerClient, ICredentialsFactory credentialsFactory, IExtendedTracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
            this.azureResourceManagerClient = azureResourceManagerClient;
        }

        /// <summary>
        /// Retrieves the Activity Log entries for <paramref name="resourceIdentifier"/> in the specified time range.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier to get the Activity Log entries for</param>
        /// <param name="startTime">The start time of the period to query Activity Log entries for</param>
        /// <param name="endTime">The end time of the period to query Activity Log entries for</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete</param>
        /// <returns>A <see cref="Task"/>, returning the Activity Log entries as a list of <see cref="JObject"/></returns>
        public async Task<List<JObject>> GetActivityLogAsync(ResourceIdentifier resourceIdentifier, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            this.tracer.TraceInformation($"Running Activity Log query on resource Id {resourceIdentifier.ToResourceId()}, start time {startTime:u}, end time  {endTime:u}.");

            // Send the request iteratively using paging, and return the response as JObject
            List<JObject> allItems = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                "GetActivityLogAsync",
                () => this.ReadAllActivityLogPagesAsync(resourceIdentifier, startTime, endTime, cancellationToken));

            this.tracer.TraceInformation($"Query completed with {allItems.Count} items");
            return allItems;
        }

        /// <summary>
        /// Build the appropriate request path according to the resource type
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier we want to build the request path for</param>
        /// <param name="startTime">The start time of the period we want to investigate</param>
        /// <param name="endTime">The end time of the period we want to investigate</param>
        /// <returns>The request path</returns>
        private static string BuildRequestPath(ResourceIdentifier resourceIdentifier, DateTime startTime, DateTime endTime)
        {
            string queryString = $@"subscriptions/{resourceIdentifier.SubscriptionId}/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01&$filter=eventTimestamp%20ge%20'{startTime:u}'%20and%20eventTimestamp%20le%20'{endTime:u}'";

            // Create the request path according to the resource type
            switch (resourceIdentifier.ResourceType)
            {
                case ResourceType.Subscription:
                    return queryString;
                case ResourceType.ResourceGroup:
                    return queryString + $"%20and%20resourceGroupName%20eq%20'{resourceIdentifier.ResourceGroupName}";
                default:
                    return queryString + $"%20and%20resourceUri%20eq%20'{resourceIdentifier.ToResourceId()}";
            }
        }

        /// <summary>
        /// Enumerate all Activity Log objects using the nextLink URI provided by the API
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier to get the Activity Log entries for</param>
        /// <param name="startTime">The start time of the period to query Activity Log entries for</param>
        /// <param name="endTime">The end time of the period to query Activity Log entries for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>List of all objects</returns>
        private async Task<List<JObject>> ReadAllActivityLogPagesAsync(ResourceIdentifier resourceIdentifier, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            string requestPath = BuildRequestPath(resourceIdentifier, startTime, endTime);
            return await this.azureResourceManagerClient.ExecuteArmRequestAsync(requestPath, cancellationToken);
        }
    }
}
