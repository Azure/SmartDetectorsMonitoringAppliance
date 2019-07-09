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
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
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

        private readonly SmartDetectorManifest smartDetectorManifest;

        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;

        private readonly IPageableLogArchive logArchive;

        private ObservableCollection<EmulationAlert> alerts;

        private bool isSmartDetectorRunning;

        private IPageableLog pageableLogTracer;

        private Action cancelSmartDetectorRunAction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunner"/> class.
        /// </summary>
        /// <param name="smartDetector">The Smart Detector.</param>
        /// <param name="analysisServicesFactory">The analysis services factory.</param>
        /// <param name="smartDetectorManifest">The Smart Detector manifest.</param>
        /// <param name="stateRepositoryFactory">The state repository factory</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="logArchive">The log archive.</param>
        public SmartDetectorRunner(
            ISmartDetector smartDetector,
            IInternalAnalysisServicesFactory analysisServicesFactory,
            SmartDetectorManifest smartDetectorManifest,
            IStateRepositoryFactory stateRepositoryFactory,
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            IPageableLogArchive logArchive)
        {
            this.smartDetector = smartDetector;
            this.analysisServicesFactory = analysisServicesFactory;
            this.smartDetectorManifest = smartDetectorManifest;
            this.logArchive = logArchive;
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
        /// Gets a value indicating whether the Smart Detector is running.
        /// </summary>
        public bool IsSmartDetectorRunning
        {
            get => this.isSmartDetectorRunning;

            private set
            {
                this.isSmartDetectorRunning = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the log used for the last (or current) run.
        /// </summary>
        public IPageableLog PageableLog
        {
            get => this.pageableLogTracer;

            private set
            {
                this.pageableLogTracer = value;
                this.OnPropertyChanged();
            }
        }

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
                CancellationToken cancellationToken = cancellationTokenSource.Token;
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
                    for (DateTime currentTime = startTimeRange; currentTime <= endTimeRange; currentTime = currentTime.Add(analysisCadence))
                    {
                        this.PageableLog = await this.logArchive.GetLogAsync(this.GetValidLogName(currentTime), 50);
                        using (ILogArchiveTracer tracer = this.PageableLog.CreateTracer())
                        {
                            try
                            {
                                tracer.TraceInformation($"Start analysis, with session ID = '{tracer.SessionId}' end of time range: {currentTime}");

                                ExtendedDateTime.SetEmulatedUtcNow(currentTime);
                                var analysisRequest = new AnalysisRequest(
                                    new AnalysisRequestParameters(ExtendedDateTime.UtcNow, targetResourcesForDetector, analysisCadence, null, null),
                                    this.analysisServicesFactory,
                                    stateRepository);

                                // Run the detector in a different context by using "Task.Run()". This will prevent the detector execution from blocking the UI
                                List<Alert> newAlerts = await Task.Run(() =>
                                    this.smartDetector.AnalyzeResourcesAsync(
                                        analysisRequest,
                                        tracer,
                                        cancellationToken));

                                var smartDetectorExecutionRequest = new SmartDetectorAnalysisRequest
                                {
                                    ResourceIds = targetResourcesIds,
                                    SmartDetectorId = this.smartDetectorManifest.Id,
                                    Cadence = analysisCadence,
                                };

                                foreach (Alert newAlert in newAlerts)
                                {
                                    ContractsAlert contractsAlert = newAlert.CreateContractsAlert(
                                        smartDetectorExecutionRequest,
                                        this.smartDetectorManifest.Name,
                                        this.analysisServicesFactory.UsedLogAnalysisClient,
                                        this.analysisServicesFactory.UsedMetricClient);
                                    this.Alerts.Add(new EmulationAlert(contractsAlert, currentTime));
                                }

                                tracer.TraceInformation($"Completed {currentRunNumber} of {totalRunsAmount} runs - found {newAlerts.Count} new alerts");
                                currentRunNumber++;
                            }
                            catch (OperationCanceledException)
                            {
                                tracer.TraceError("Smart Detector run was canceled.");
                                break;
                            }
                            catch (Exception e)
                            {
                                tracer.TraceError($"Got exception while running detector: {e}");
                            }
                        }
                    }
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
                IEnumerable<IGrouping<string, ResourceIdentifier>> resourcesByGroups = allResources.GroupBy(resource => resource.ResourceGroupName);
                IGrouping<string, ResourceIdentifier> targetResourceGroup = resourcesByGroups.First(group => group.Key == targetResource.ResourceIdentifier.ResourceGroupName);

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
        /// Gets a valid log name for running the detector on the specified emulated time
        /// </summary>
        /// <param name="emulatedTime">The detector's run emulated time.</param>
        /// <returns>The log name for the run.</returns>
        private string GetValidLogName(DateTime emulatedTime)
        {
            string logName = $"{emulatedTime:yyyy-MM-dd HH-mm}";
            int index = 0;
            while (this.logArchive.LogNames.Contains(logName))
            {
                logName = $"{emulatedTime:yyyy-MM-dd HH-mm} ({index})";
                index++;
            }

            return logName;
        }
    }
}
