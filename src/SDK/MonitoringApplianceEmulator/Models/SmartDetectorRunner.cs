//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunner.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// An observable class that runs a Smart Detector.
    /// </summary>
    public class SmartDetectorRunner : ObservableObject, IEmulationSmartDetectorRunner
    {
        private readonly IStateRepositoryFactory stateRepositoryFactory;

        private readonly ISmartDetector smartDetector;

        private readonly IInternalAnalysisServicesFactory analysisServicesFactory;

        private readonly IQueryRunInfoProvider queryRunInfoProvider;

        private readonly SmartDetectorManifest smartDetectorManifest;

        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;

        private ObservableCollection<EmulationAlert> alerts;

        private bool isSmartDetectorRunning;

        private Action cancelSmartDetectorRunAction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunner"/> class.
        /// </summary>
        /// <param name="smartDetector">The Smart Detector.</param>
        /// <param name="analysisServicesFactory">The analysis services factory.</param>
        /// <param name="queryRunInfoProvider">The query run information provider.</param>
        /// <param name="smartDetectorManifes">The Smart Detector manifest.</param>
        /// <param name="stateRepositoryFactory">The state repository factory</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="tracer">The tracer.</param>
        public SmartDetectorRunner(
            ISmartDetector smartDetector,
            IInternalAnalysisServicesFactory analysisServicesFactory,
            IQueryRunInfoProvider queryRunInfoProvider,
            SmartDetectorManifest smartDetectorManifes,
            IStateRepositoryFactory stateRepositoryFactory,
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            ITracer tracer)
        {
            this.smartDetector = smartDetector;
            this.analysisServicesFactory = analysisServicesFactory;
            this.queryRunInfoProvider = queryRunInfoProvider;
            this.smartDetectorManifest = smartDetectorManifes;
            this.Tracer = tracer;
            this.IsSmartDetectorRunning = false;
            this.Alerts = new ObservableCollection<EmulationAlert>();
            this.stateRepositoryFactory = stateRepositoryFactory;
            this.azureResourceManagerClient = azureResourceManagerClient;
        }

        #region Binded Properties

        /// <summary>
        /// Gets the Smart Detector run's alerts.
        /// </summary>
        public ObservableCollection<EmulationAlert> Alerts
        {
            get => this.alerts;

            private set
            {
                this.alerts = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Smart Detector is running.
        /// </summary>
        public bool IsSmartDetectorRunning
        {
            get => this.isSmartDetectorRunning;

            set
            {
                this.isSmartDetectorRunning = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the tracer used by the Smart Detector runner.
        /// </summary>
        private ITracer Tracer { get; }

        #endregion

        #region IEmulationSmartDetectorRunner implementation

        /// <summary>
        /// Runs the Smart Detector.
        /// </summary>
        /// <param name="targetResource">The resource which the Smart Detector should run on</param>
        /// <param name="allResources">All supported resources in subscription</param>
        /// <param name="analysisCadence">The analysis cadence</param>
        /// <param name="startTimeRange">The start time</param>
        /// <param name="endTimeRange">The end time</param>
        /// <returns>A task that runs the Smart Detector</returns>
        public async Task RunAsync(HierarchicalResource targetResource, List<ResourceIdentifier> allResources, TimeSpan analysisCadence, DateTime startTimeRange, DateTime endTimeRange)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                this.cancelSmartDetectorRunAction = () => cancellationTokenSource.Cancel();
                var cancellationToken = cancellationTokenSource.Token;
                IStateRepository stateRepository = this.stateRepositoryFactory.Create(this.smartDetectorManifest.Id, "EmulationAlertRule");

                this.Alerts.Clear();
                try
                {
                    // Run Smart Detector
                    this.IsSmartDetectorRunning = true;

                    List<ResourceIdentifier> targetResourcesForDetector = this.GetTargetResourcesForDetector(targetResource, allResources);
                    List<string> targetResourcesIds = targetResourcesForDetector.Select(resource => resource.ToResourceId()).ToList();

                    int totalRunsAmount = (int)((endTimeRange.Subtract(startTimeRange).Ticks / analysisCadence.Ticks) + 1);
                    int currentRunNumber = 1;
                    for (var currentTime = startTimeRange; currentTime <= endTimeRange; currentTime = currentTime.Add(analysisCadence))
                    {
                        this.Tracer.TraceInformation($"Start analysis, end of time range: {currentTime}");

                        ExtendedDateTime.SetEmulatedUtcNow(currentTime);
                        var analysisRequest = new AnalysisRequest(targetResourcesForDetector, analysisCadence, null, this.analysisServicesFactory, stateRepository);

                        // Run the detector in a different context by using "Task.Run()". This will prevent the detector execution from blocking the UI
                        List<SmartDetectors.Alert> newAlerts = await Task.Run(() => this.smartDetector.AnalyzeResourcesAsync(
                            analysisRequest,
                            this.Tracer,
                            cancellationToken));

                        var smartDetectorExecutionRequest = new SmartDetectorExecutionRequest
                        {
                            ResourceIds = targetResourcesIds,
                            SmartDetectorId = this.smartDetectorManifest.Id,
                            Cadence = analysisCadence,
                        };

                        var lazyResourceToWorkspaceResourceIdMapping = new Lazy<Task<Dictionary<ResourceIdentifier, ResourceIdentifier>>>(() => this.GetResourceToWorkspaceResourceIdMappingAsync(targetResourcesForDetector, cancellationToken));

                        foreach (var newAlert in newAlerts)
                        {
                            QueryRunInfo queryRunInfo = await this.CreateQueryRunInfoForAlertAsync(newAlert, lazyResourceToWorkspaceResourceIdMapping, cancellationToken);
                            ContractsAlert contractsAlert = newAlert.CreateContractsAlert(smartDetectorExecutionRequest, this.smartDetectorManifest.Name, queryRunInfo, this.analysisServicesFactory.UsedLogAnalysisClient, this.analysisServicesFactory.UsedMetricClient);
                            this.Alerts.Add(new EmulationAlert(contractsAlert, currentTime));
                        }

                        this.Tracer.TraceInformation($"completed {currentRunNumber} of {totalRunsAmount} runs");
                        currentRunNumber++;
                    }

                    string separator = "=====================================================================================================";
                    this.Tracer.TraceInformation($"Total alerts found: {this.Alerts.Count} {Environment.NewLine} {separator}");
                }
                catch (OperationCanceledException)
                {
                    this.Tracer.TraceError("Smart Detector run was canceled.");
                }
                catch (Exception e)
                {
                    this.Tracer.TraceError($"Got exception while running detector: {e}");
                }
                finally
                {
                    this.IsSmartDetectorRunning = false;
                    this.cancelSmartDetectorRunAction = null;
                }
            }
        }

        /// <summary>
        /// Cancels the Smart Detector run.
        /// </summary>
        public void CancelSmartDetectorRun()
        {
            this.cancelSmartDetectorRunAction?.Invoke();
        }

        #endregion

        /// <summary>
        /// Verify that the target resource type is supported by the Smart Detector, and enumerate
        /// the resources that the Smart Detector should run on.
        /// </summary>
        /// <param name="targetResource">The selected target resource</param>
        /// <param name="allResources">All supported resources in subscription</param>
        /// <returns>The resource identifiers that the Smart Detector should run on</returns>
        private List<ResourceIdentifier> GetTargetResourcesForDetector(HierarchicalResource targetResource, List<ResourceIdentifier> allResources)
        {
            List<ResourceIdentifier> targetResourcesForDetector = new List<ResourceIdentifier>();

            if (this.smartDetectorManifest.SupportedResourceTypes.Contains(targetResource.ResourceIdentifier.ResourceType))
            {
                targetResourcesForDetector.Add(targetResource.ResourceIdentifier);
            }
            else if (targetResource.ResourceIdentifier.ResourceType == ResourceType.Subscription && this.smartDetectorManifest.SupportedResourceTypes.Contains(ResourceType.ResourceGroup))
            {
                // If the request is for a subscription, and the Smart Detector supports a resource group type, enumerate all resource groups in the requested subscription
                List<ResourceIdentifier> resourceGroups = allResources.Where(resource => resource.ResourceType == ResourceType.ResourceGroup).ToList();
                targetResourcesForDetector.AddRange(resourceGroups);
            }
            else if (targetResource.ResourceIdentifier.ResourceType == ResourceType.Subscription)
            {
                // If the request is for a subscription, enumerate all the *supported* resources in the requested subscription
                List<ResourceIdentifier> resources = allResources.Where(resource => this.smartDetectorManifest.SupportedResourceTypes.Contains(resource.ResourceType)).ToList();
                targetResourcesForDetector.AddRange(resources);
            }
            else if (targetResource.ResourceIdentifier.ResourceType == ResourceType.ResourceGroup && this.smartDetectorManifest.SupportedResourceTypes.Any(type => type != ResourceType.Subscription))
            {
                // If the request is for a resource group, and the Smart Detector supports resource types (other than subscription),
                // enumerate all the *supported* resources in the requested resource group
                var resourcesByGroups = allResources.GroupBy(resource => resource.ResourceGroupName);
                var targetResourceGroup = resourcesByGroups.First(group => group.Key == targetResource.ResourceIdentifier.ResourceGroupName);

                List<ResourceIdentifier> resources = targetResourceGroup.ToList()
                    .Where(resource => this.smartDetectorManifest.SupportedResourceTypes.Contains(resource.ResourceType))
                    .ToList();
                targetResourcesForDetector.AddRange(resources);
            }
            else
            {
                // The Smart Detector does not support the requested resource type
                throw new ArgumentException($"The Smart detector does not support the requested resource type: {targetResource.ResourceIdentifier.ResourceType}");
            }

            return targetResourcesForDetector;
        }

        /// <summary>
        /// Creates <see cref="QueryRunInfo"/> for alert based on alert resource id and a mapping between resources and workspaces
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="lazyResourceToWorkspaceResourceIdMapping">Lazily computed mapping between resources and workspaces</param>
        /// <param name="cancellationToken">A cancellation token controlling the asynchronous operation</param>
        /// <returns>A task returning <see cref="QueryRunInfo"/> for alert</returns>
        private async Task<QueryRunInfo> CreateQueryRunInfoForAlertAsync(
            SmartDetectors.Alert alert,
            Lazy<Task<Dictionary<ResourceIdentifier, ResourceIdentifier>>> lazyResourceToWorkspaceResourceIdMapping,
            CancellationToken cancellationToken)
        {
            if (alert.ResourceIdentifier.ResourceType != ResourceType.ApplicationInsights)
            {
                Dictionary<ResourceIdentifier, ResourceIdentifier> resourceToWorkspaceResourceIdMapping = await lazyResourceToWorkspaceResourceIdMapping.Value;
                if (resourceToWorkspaceResourceIdMapping.ContainsKey(alert.ResourceIdentifier))
                {
                    return new QueryRunInfo
                    {
                        ResourceIds = new List<string> { resourceToWorkspaceResourceIdMapping[alert.ResourceIdentifier].ToResourceId() },
                        Type = TelemetryDbType.LogAnalytics
                    };
                }
            }

            return await this.queryRunInfoProvider.GetQueryRunInfoAsync(new List<ResourceIdentifier>() { alert.ResourceIdentifier }, cancellationToken);
        }

        /// <summary>
        /// Creates a mapping between resources and workspaces
        /// </summary>
        /// <param name="detectorResources">Resources that were provided to Smart Detector execution</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task returning a mapping between resources and workspaces</returns>
        private async Task<Dictionary<ResourceIdentifier, ResourceIdentifier>> GetResourceToWorkspaceResourceIdMappingAsync(IReadOnlyList<ResourceIdentifier> detectorResources, CancellationToken cancellationToken)
        {
            Dictionary<ResourceIdentifier, string> resourceToWorkspaceIdMapping = await this.CreateResourceToWorkspaceIdMappingAsync(detectorResources, cancellationToken);

            QueryRunInfo detectorResourcesQueryRunInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(detectorResources, cancellationToken);
            List<ResourceIdentifier> workspaces = detectorResourcesQueryRunInfo.ResourceIds.Select(ResourceIdentifier.CreateFromResourceId).ToList();

            Dictionary<string, ResourceIdentifier> workspaceIdToWorkspaceResourceIdMapping = await this.CreateWorkspaceIdToWorkspaceResourceIdMappingAsync(workspaces, cancellationToken);

            Dictionary<ResourceIdentifier, ResourceIdentifier> resourceToWorkspaceResourceIdMapping
                = resourceToWorkspaceIdMapping
                .Where(pair => workspaceIdToWorkspaceResourceIdMapping.ContainsKey(pair.Value))
                .ToDictionary(pair => pair.Key, pair => workspaceIdToWorkspaceResourceIdMapping[pair.Value]);

            return resourceToWorkspaceResourceIdMapping;
        }

        /// <summary>
        /// Creates a mapping between resources and workspace ids that these resources are connected to.
        /// The workspaces must be located in scopes defined by <paramref name="resources"/> in order for them (and resources connected to them) to appear in the mapping.
        /// </summary>
        /// <param name="resources">The resources</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task returning a mapping between resources and workspace ids that these resources are connected to</returns>
        private async Task<Dictionary<ResourceIdentifier, string>> CreateResourceToWorkspaceIdMappingAsync(IReadOnlyList<ResourceIdentifier> resources, CancellationToken cancellationToken)
        {
            ITelemetryDataClient dataClient = await this.analysisServicesFactory.CreateLogAnalyticsTelemetryDataClientAsync(resources, cancellationToken);

            Dictionary<ResourceIdentifier, string> resourceToWorkspaceIdMapping = new Dictionary<ResourceIdentifier, string>();

            string query = $@"
                            Heartbeat
                            | where TimeGenerated >= datetime({ExtendedDateTime.UtcNow.AddDays(-1):u}) and TimeGenerated <= datetime({ExtendedDateTime.UtcNow:u}) 
                            | where isnotempty(SubscriptionId) and isnotempty(ResourceGroup) and isnotempty(Resource)
                            | parse Resource with ResourcePrefix '_' *
                            | extend ResourceName = iff(isnotempty(ResourcePrefix), ResourcePrefix, Resource)
                            | extend ResourceType = iff(isnotempty(ResourcePrefix), '{ResourceType.VirtualMachineScaleSet.ToString()}', '{ResourceType.VirtualMachine.ToString()}')
                            | extend WorkspaceId = TenantId
                            | summarize by WorkspaceId = tolower(WorkspaceId), SubscriptionId = tolower(SubscriptionId), ResourceGroup = tolower(ResourceGroup), ResourceName = tolower(ResourceName), ResourceType";

            IList<DataTable> dataTables = await dataClient.RunQueryAsync(query, cancellationToken);
            foreach (DataRow row in dataTables[0].Rows)
            {
                var resorceIdentifier = new ResourceIdentifier(
                    (ResourceType)Enum.Parse(typeof(ResourceType), row["ResourceType"].ToString()),
                    row["SubscriptionId"].ToString(),
                    row["ResourceGroup"].ToString(),
                    row["ResourceName"].ToString());

                string workspaceId = row["WorkspaceId"].ToString();

                resourceToWorkspaceIdMapping[resorceIdentifier] = workspaceId;
            }

            return resourceToWorkspaceIdMapping;
        }

        /// <summary>
        /// Creates a mapping between workspace ids and workspace resource ids
        /// </summary>
        /// <param name="workspaces">The workspaces to map</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task returning a mapping between workspace ids and workspace resource ids</returns>
        private async Task<Dictionary<string, ResourceIdentifier>> CreateWorkspaceIdToWorkspaceResourceIdMappingAsync(IReadOnlyList<ResourceIdentifier> workspaces, CancellationToken cancellationToken)
        {
            Dictionary<string, ResourceIdentifier> workspaceIdToWorkspaceResourceIdMapping = new Dictionary<string, ResourceIdentifier>();

            foreach (var workspace in workspaces)
            {
                try
                {
                    string workspaceId = await this.azureResourceManagerClient.GetLogAnalyticsWorkspaceIdAsync(workspace, cancellationToken);
                    workspaceIdToWorkspaceResourceIdMapping[workspaceId] = workspace;
                }
                catch (AzureResourceManagerClientException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // ignore missing workspaces
                }
            }

            return workspaceIdToWorkspaceResourceIdMapping;
        }
    }
}
