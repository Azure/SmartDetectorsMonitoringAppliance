//-----------------------------------------------------------------------
// <copyright file="QueryRunInfoProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// An implementation of the <see cref="IQueryRunInfoProvider"/> interface.
    /// </summary>
    public class QueryRunInfoProvider : IQueryRunInfoProvider
    {
        private const int MaxNumberOfResourcesInQuery = 10;

        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;
        private readonly ConcurrentDictionary<string, IList<ResourceIdentifier>> subscriptionIdToWorkspaces = new ConcurrentDictionary<string, IList<ResourceIdentifier>>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunInfoProvider"/> class
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        public QueryRunInfoProvider(IExtendedAzureResourceManagerClient azureResourceManagerClient)
        {
            this.azureResourceManagerClient = azureResourceManagerClient;
        }

        /// <summary>
        /// Gets the run information to query telemetry for the specified resources
        /// </summary>
        /// <param name="resources">The resources</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the query run information</returns>
        public async Task<QueryRunInfo> GetQueryRunInfoAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            // Verify that there are resources
            if (!resources.Any())
            {
                throw new QueryClientInfoProviderException("No resources provided");
            }

            // Verify that there are no Application Insights resources
            if (resources.All(resource => resource.ResourceType != ResourceType.ApplicationInsights))
            {
                IReadOnlyList<ResourceIdentifier> workspaces;
                if (resources.All(resource => resource.ResourceType == ResourceType.LogAnalytics))
                {
                    // All resources are of type LogAnalytics. Create a client that queries all these workspaces.
                    workspaces = resources;
                }
                else
                {
                    // Since we do not know where the telemetry of each resource is, create a client that queries all workspaces in the subscription.
                    var workspacesList = new List<ResourceIdentifier>();
                    foreach (string subscriptionId in resources.Select(resource => resource.SubscriptionId).Distinct(StringComparer.CurrentCultureIgnoreCase))
                    {
                        // Try to get the workspaces list from the cache, and if it isn't there, use the Azure Resource Manager client
                        IList<ResourceIdentifier> subscriptionWorkspaces;
                        if (!this.subscriptionIdToWorkspaces.TryGetValue(subscriptionId, out subscriptionWorkspaces))
                        {
                            subscriptionWorkspaces = await this.azureResourceManagerClient.GetAllResourcesInSubscriptionAsync(subscriptionId, new[] { ResourceType.LogAnalytics }, cancellationToken);
                            this.subscriptionIdToWorkspaces[subscriptionId] = subscriptionWorkspaces;
                        }

                        workspacesList.AddRange(subscriptionWorkspaces);
                    }

                    workspaces = workspacesList;
                    if (workspaces.Count == 0)
                    {
                        throw new InvalidOperationException("No log analytics workspaces were found");
                    }
                }

                // Verify there are not too many resources
                if (resources.Count > MaxNumberOfResourcesInQuery)
                {
                    throw new QueryClientInfoProviderException($"Cannot run analysis on more than {MaxNumberOfResourcesInQuery} applications");
                }

                List<string> workspacesResourceIds = workspaces.Select(workspace => workspace.ToResourceId()).ToList();
                return new QueryRunInfo
                {
                    Type = TelemetryDbType.LogAnalytics,
                    ResourceIds = workspacesResourceIds
                };
            }
            else
            {
                // Verify that all resources are of type ApplicationInsights
                if (resources.Any(resource => resource.ResourceType != ResourceType.ApplicationInsights))
                {
                    throw new QueryClientInfoProviderException("An Application Insights telemetry data client can only be created for resources of type ApplicationInsights");
                }

                // Verify there are not too many resources
                if (resources.Count > MaxNumberOfResourcesInQuery)
                {
                    throw new QueryClientInfoProviderException($"Cannot run analysis on more than {MaxNumberOfResourcesInQuery} applications");
                }

                List<string> applicationsResourceIds = resources.Select(application => application.ToResourceId()).ToList();
                return new QueryRunInfo
                {
                    Type = TelemetryDbType.ApplicationInsights,
                    ResourceIds = applicationsResourceIds
                };
            }
        }
    }
}