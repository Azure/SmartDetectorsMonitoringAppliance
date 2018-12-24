﻿//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunner.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using AlertResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.AlertResolutionCheckRequest;
    using AlertResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.AlertResolutionCheckResponse;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsAlertResolutionCheckRequest = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertResolutionCheckRequest;
    using ContractsAlertResolutionCheckResponse = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertResolutionCheckResponse;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    /// <summary>
    /// An implementation of <see cref="ISmartDetectorRunner"/>, that loads the Smart Detector and runs it
    /// </summary>
    public class SmartDetectorRunner : ISmartDetectorRunner
    {
        private readonly ISmartDetectorRepository smartDetectorRepository;
        private readonly ISmartDetectorLoader smartDetectorLoader;
        private readonly IInternalAnalysisServicesFactory analysisServicesFactory;
        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;
        private readonly IQueryRunInfoProvider queryRunInfoProvider;
        private readonly IStateRepositoryFactory stateRepositoryFactory;
        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunner"/> class
        /// </summary>
        /// <param name="smartDetectorRepository">The Smart Detector repository</param>
        /// <param name="smartDetectorLoader">The Smart Detector loader</param>
        /// <param name="analysisServicesFactory">The analysis services factory</param>
        /// <param name="azureResourceManagerClient">The Azure Resource Manager client</param>
        /// <param name="queryRunInfoProvider">The query run information provider</param>
        /// <param name="stateRepositoryFactory">The state repository factory</param>
        /// <param name="tracer">The tracer</param>
        public SmartDetectorRunner(
            ISmartDetectorRepository smartDetectorRepository,
            ISmartDetectorLoader smartDetectorLoader,
            IInternalAnalysisServicesFactory analysisServicesFactory,
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            IQueryRunInfoProvider queryRunInfoProvider,
            IStateRepositoryFactory stateRepositoryFactory,
            IExtendedTracer tracer)
        {
            this.smartDetectorRepository = Diagnostics.EnsureArgumentNotNull(() => smartDetectorRepository);
            this.smartDetectorLoader = Diagnostics.EnsureArgumentNotNull(() => smartDetectorLoader);
            this.analysisServicesFactory = Diagnostics.EnsureArgumentNotNull(() => analysisServicesFactory);
            this.azureResourceManagerClient = Diagnostics.EnsureArgumentNotNull(() => azureResourceManagerClient);
            this.queryRunInfoProvider = Diagnostics.EnsureArgumentNotNull(() => queryRunInfoProvider);
            this.stateRepositoryFactory = Diagnostics.EnsureArgumentNotNull(() => stateRepositoryFactory);
            this.tracer = tracer;
        }

        #region Implementation of ISmartDetectorRunner

        /// <summary>
        /// Loads the Smart Detector, runs its analysis flow, and returns the generated alert.
        /// </summary>
        /// <param name="request">The Smart Detector analysis request</param>
        /// <param name="shouldDetectorTrace">Determines if the detector's traces are emitted</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the list of Alerts generated by the Smart Detector.</returns>
        public async Task<List<ContractsAlert>> AnalyzeAsync(SmartDetectorAnalysisRequest request, bool shouldDetectorTrace, CancellationToken cancellationToken)
        {
            return await this.LoadAndRunSmartDetector(
                request.SmartDetectorId,
                shouldDetectorTrace,
                request,
                this.AnalyzeAsync,
                cancellationToken);
        }

        /// <summary>
        /// Runs the Smart Detector's resolution check flow.
        /// </summary>
        /// <param name="request">The alert resolution check request.</param>
        /// <param name="shouldDetectorTrace">Determines if the detector's traces are emitted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resolution check response generated by the Smart Detector.</returns>
        public async Task<ContractsAlertResolutionCheckResponse> CheckResolutionAsync(
            ContractsAlertResolutionCheckRequest request,
            bool shouldDetectorTrace,
            CancellationToken cancellationToken)
        {
            return await this.LoadAndRunSmartDetector(
                request.OriginalAnalysisRequest.SmartDetectorId,
                shouldDetectorTrace,
                request,
                this.CheckAlertResolutionAsync,
                cancellationToken);
        }

        #endregion

        /// <summary>
        /// Gets the key used for storing the resolution state for the given Alert Correlation Hash.
        /// </summary>
        /// <param name="alertCorrelationHash">The Alert Correlation Hash.</param>
        /// <returns>The key used for storing the resolution state the given Alert Correlation Hash.</returns>
        private static string GetResolutionStateKey(string alertCorrelationHash)
        {
            return $"_autoResolve{alertCorrelationHash}";
        }

        /// <summary>
        /// Runs the Smart Detector's analysis flow.
        /// </summary>
        /// <param name="request">The Smart Detector analysis request</param>
        /// <param name="smartDetector">The Smart Detector to run</param>
        /// <param name="smartDetectorManifest">The Smart Detector's manifest</param>
        /// <param name="detectorTracer">The tracer to provider for the Smart Detector</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the list of Alerts generated by the Smart Detector.</returns>
        private async Task<List<ContractsAlert>> AnalyzeAsync(
            SmartDetectorAnalysisRequest request,
            ISmartDetector smartDetector,
            SmartDetectorManifest smartDetectorManifest,
            ITracer detectorTracer,
            CancellationToken cancellationToken)
        {
            // Create state repository
            IStateRepository stateRepository = this.stateRepositoryFactory.Create(request.SmartDetectorId, request.AlertRuleResourceId);

            // Create the input for the Smart Detector
            AnalysisRequestParameters analysisRequestParameters = await this.CreateAnalysisRequestParametersAsync(request, smartDetectorManifest, true, cancellationToken);
            var analysisRequest = new AnalysisRequest(analysisRequestParameters, this.analysisServicesFactory, stateRepository);

            // Run the Smart Detector
            this.tracer.TraceInformation($"Started running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name}");
            List<Alert> alerts;
            try
            {
                alerts = await smartDetector.AnalyzeResourcesAsync(analysisRequest, detectorTracer, cancellationToken);
                this.tracer.TraceInformation(
                    $"Completed running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name}, returning {alerts.Count} alerts");
            }
            catch (DetectorDataNotReadyException ddnre)
            {
                this.tracer.TraceWarning($"Smart Detector data is not ready yet, aborting analysis: {ddnre.Message}");
                return new List<ContractsAlert>();
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Failed running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name}: {e}");
                throw new FailedToRunSmartDetectorException($"Calling Smart Detector '{smartDetectorManifest.Name}' failed with exception of type {e.GetType()} and message: {e.Message}", e);
            }

            // Verify that each alert belongs to one of the types declared in the Smart Detector manifest
            foreach (Alert alert in alerts)
            {
                if (!smartDetectorManifest.SupportedResourceTypes.Contains(alert.ResourceIdentifier.ResourceType))
                {
                    throw new UnidentifiedAlertResourceTypeException(alert.ResourceIdentifier);
                }
            }

            // Trace the number of alerts of each type
            foreach (var alertType in alerts.GroupBy(x => x.GetType().Name))
            {
                this.tracer.TraceInformation($"Got {alertType.Count()} Alerts of type '{alertType.Key}'");
                this.tracer.ReportMetric("AlertType", alertType.Count(), new Dictionary<string, string>() { { "AlertType", alertType.Key } });
            }

            // Create results
            bool detectorSupportsAlertResolution = smartDetector is IAlertResolutionSmartDetector;
            List<ContractsAlert> results = new List<ContractsAlert>();
            foreach (var alert in alerts)
            {
                QueryRunInfo queryRunInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(new List<ResourceIdentifier>() { alert.ResourceIdentifier }, cancellationToken);
                ContractsAlert contractsAlert = alert.CreateContractsAlert(
                    request,
                    smartDetectorManifest.Name,
                    queryRunInfo,
                    this.analysisServicesFactory.UsedLogAnalysisClient,
                    this.analysisServicesFactory.UsedMetricClient);

                // Handle resolution parameters in the alerts:
                // If the detector supports resolution - save the predicates for the resolution checks
                // If the detector doesn't support resolution - drop the resolution parameters (since they are useless) and error trace
                if (contractsAlert.ResolutionParameters != null)
                {
                    if (detectorSupportsAlertResolution)
                    {
                        this.tracer.TraceInformation($"Alert {contractsAlert.Id} has resolution parameters, so saving alert details for later use");
                        await stateRepository.StoreStateAsync(
                            GetResolutionStateKey(contractsAlert.Id),
                            new ResolutionState
                            {
                                AlertPredicates = alert.ExtractPredicates()
                            },
                            cancellationToken);
                    }
                    else
                    {
                        this.tracer.TraceError($"Dropping resolution parameters from alert {contractsAlert.Id}");
                        contractsAlert.ResolutionParameters = null;
                    }
                }

                // And add the alert to the results
                results.Add(contractsAlert);
            }

            this.tracer.TraceInformation($"Returning {results.Count} results");
            return results;
        }

        /// <summary>
        /// Runs the Smart Detector's resolution check flow.
        /// </summary>
        /// <param name="request">The alert resolution check request.</param>
        /// <param name="smartDetector">The Smart Detector to run</param>
        /// <param name="smartDetectorManifest">The Smart Detector's manifest</param>
        /// <param name="detectorTracer">The tracer to provider for the Smart Detector</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resolution check response generated by the Smart Detector.</returns>
        private async Task<ContractsAlertResolutionCheckResponse> CheckAlertResolutionAsync(
            ContractsAlertResolutionCheckRequest request,
            ISmartDetector smartDetector,
            SmartDetectorManifest smartDetectorManifest,
            ITracer detectorTracer,
            CancellationToken cancellationToken)
        {
            // Check that the detector supports resolution
            if (!(smartDetector is IAlertResolutionSmartDetector alertResolutionSmartDetector))
            {
                throw new ResolutionCheckNotSupportedException($"Smart Detector {smartDetectorManifest.Name} does not support alert resolution of alerts");
            }

            // Create state repository
            IStateRepository stateRepository = this.stateRepositoryFactory.Create(request.OriginalAnalysisRequest.SmartDetectorId, request.OriginalAnalysisRequest.AlertRuleResourceId);

            // Load the resolution state from the repository
            ResolutionState resolutionState = await stateRepository.GetStateAsync<ResolutionState>(GetResolutionStateKey(request.AlertCorrelationHash), cancellationToken);
            if (resolutionState == null)
            {
                throw new ResolutionStateNotFoundException($"Resolution state for Alert with correlation {request.AlertCorrelationHash} was not found");
            }

            // Create the input for the Smart Detector
            AnalysisRequestParameters analysisRequestParameters = await this.CreateAnalysisRequestParametersAsync(request.OriginalAnalysisRequest, smartDetectorManifest, false, cancellationToken);
            var alertResolutionCheckRequest = new AlertResolutionCheckRequest(
                analysisRequestParameters,
                new AlertResolutionCheckRequestParameters(ResourceIdentifier.CreateFromResourceId(request.TargetResource), request.AlertFireTime, resolutionState.AlertPredicates),
                this.analysisServicesFactory,
                stateRepository);

            // Run the Smart Detector
            this.tracer.TraceInformation($"Started running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name} for resolution check");
            try
            {
                AlertResolutionCheckResponse alertResolutionCheckResponse = await alertResolutionSmartDetector.CheckForResolutionAsync(alertResolutionCheckRequest, detectorTracer, cancellationToken);
                this.tracer.TraceInformation($"Completed running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name} for resolution check");

                // If the alert is resolved - delete the state
                if (alertResolutionCheckResponse.ShouldBeResolved)
                {
                    await stateRepository.DeleteStateAsync(GetResolutionStateKey(request.AlertCorrelationHash), cancellationToken);
                }

                // Convert the result
                return new ContractsAlertResolutionCheckResponse
                {
                    ShouldBeResolved = alertResolutionCheckResponse.ShouldBeResolved,
                    ResolutionParameters = alertResolutionCheckResponse.AlertResolutionParameters?.CreateContractsResolutionParameters()
                };
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Failed running Smart Detector ID {smartDetectorManifest.Id}, Name {smartDetectorManifest.Name} for resolution check: {e}");
                throw new FailedToRunSmartDetectorException($"Calling Smart Detector '{smartDetectorManifest.Name}' for resolution check failed with exception of type {e.GetType()} and message: {e.Message}", e);
            }
        }

        /// <summary>
        /// Loads and runs a specific flow on the requested Smart Detector.
        /// </summary>
        /// <typeparam name="TIn">The flow's primary input type.</typeparam>
        /// <typeparam name="TOut">The flow's output type.</typeparam>
        /// <param name="smartDetectorId">The ID of the Smart Detector to load</param>
        /// <param name="shouldDetectorTrace">Determines if the detector's traces are emitted.</param>
        /// <param name="runnerInput">The flow's input</param>
        /// <param name="flowRunner">A function that runs the Smart Detector Flow.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the output returned from <paramref name="flowRunner"/>.</returns>
        private async Task<TOut> LoadAndRunSmartDetector<TIn, TOut>(
            string smartDetectorId,
            bool shouldDetectorTrace,
            TIn runnerInput,
            Func<TIn, ISmartDetector, SmartDetectorManifest, ITracer, CancellationToken, Task<TOut>> flowRunner,
            CancellationToken cancellationToken)
        {
            // Read the Smart Detector's package
            this.tracer.TraceInformation($"Loading Smart Detector package for Smart Detector ID {smartDetectorId}");
            SmartDetectorPackage smartDetectorPackage = await this.smartDetectorRepository.ReadSmartDetectorPackageAsync(smartDetectorId, cancellationToken);
            SmartDetectorManifest smartDetectorManifest = smartDetectorPackage.Manifest;
            this.tracer.TraceInformation($"Read Smart Detector package, ID {smartDetectorManifest.Id}, Version {smartDetectorManifest.Version}");

            // Load the Smart Detector
            ISmartDetector smartDetector = this.smartDetectorLoader.LoadSmartDetector(smartDetectorPackage);
            this.tracer.TraceInformation($"Smart Detector instance loaded successfully, ID {smartDetectorManifest.Id}");

            try
            {
                ITracer detectorTracer = shouldDetectorTrace ? this.tracer : new EmptyTracer();
                return await flowRunner(runnerInput, smartDetector, smartDetectorManifest, detectorTracer, cancellationToken);
            }
            finally
            {
                if (smartDetector is IDisposable disposableSmartDetector)
                {
                    disposableSmartDetector.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AnalysisRequestParameters"/> class, based on <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The analysis request received from Azure Monitoring back-end.</param>
        /// <param name="smartDetectorManifest">The Smart Detector's manifest, used for validations of the request.</param>
        /// <param name="shouldValidateResources">A value indicating whether we should validate that the request's resources are supported by the detector.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the analysis request parameters.</returns>
        private async Task<AnalysisRequestParameters> CreateAnalysisRequestParametersAsync(SmartDetectorAnalysisRequest request, SmartDetectorManifest smartDetectorManifest, bool shouldValidateResources, CancellationToken cancellationToken)
        {
            // Get the resources on which to run the Smart Detector
            List<ResourceIdentifier> resources = shouldValidateResources
                ? await this.GetResourcesForSmartDetector(request.ResourceIds, smartDetectorManifest, cancellationToken)
                : request.ResourceIds.Select(ResourceIdentifier.CreateFromResourceId).ToList();

            return new AnalysisRequestParameters(resources, request.Cadence, request.AlertRuleResourceId, request.DetectorParameters);
        }

        /// <summary>
        /// Verify that the request resource type is supported by the Smart Detector, and enumerate
        /// the resources that the Smart Detector should run on.
        /// </summary>
        /// <param name="requestResourceIds">The request resource Ids</param>
        /// <param name="smartDetectorManifest">The Smart Detector manifest</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers that the Smart Detector should run on</returns>
        private async Task<List<ResourceIdentifier>> GetResourcesForSmartDetector(IList<string> requestResourceIds, SmartDetectorManifest smartDetectorManifest, CancellationToken cancellationToken)
        {
            HashSet<ResourceIdentifier> resourcesForSmartDetector = new HashSet<ResourceIdentifier>();
            foreach (string requestResourceId in requestResourceIds)
            {
                ResourceIdentifier requestResource = ResourceIdentifier.CreateFromResourceId(requestResourceId);

                if (smartDetectorManifest.SupportedResourceTypes.Contains(requestResource.ResourceType))
                {
                    // If the Smart Detector directly supports the requested resource type, then that's it
                    resourcesForSmartDetector.Add(requestResource);
                }
                else if (requestResource.ResourceType == ResourceType.Subscription && smartDetectorManifest.SupportedResourceTypes.Contains(ResourceType.ResourceGroup))
                {
                    // If the request is for a subscription, and the Smart Detector supports a resource group type, enumerate all resource groups in the requested subscription
                    IList<ResourceIdentifier> resourceGroups = await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(requestResource.SubscriptionId, cancellationToken);
                    resourcesForSmartDetector.UnionWith(resourceGroups);
                    this.tracer.TraceInformation($"Added {resourceGroups.Count} resource groups found in subscription {requestResource.SubscriptionId}");
                }
                else if (requestResource.ResourceType == ResourceType.Subscription)
                {
                    // If the request is for a subscription, enumerate all the resources in the requested subscription that the Smart Detector supports
                    IList<ResourceIdentifier> resources = await this.azureResourceManagerClient.GetAllResourcesInSubscriptionAsync(requestResource.SubscriptionId, smartDetectorManifest.SupportedResourceTypes, cancellationToken);
                    resourcesForSmartDetector.UnionWith(resources);
                    this.tracer.TraceInformation($"Added {resources.Count} resources found in subscription {requestResource.SubscriptionId}");
                }
                else if (requestResource.ResourceType == ResourceType.ResourceGroup && smartDetectorManifest.SupportedResourceTypes.Any(type => type != ResourceType.Subscription))
                {
                    // If the request is for a resource group, and the Smart Detector supports resource types (other than subscription),
                    // enumerate all the resources in the requested resource group that the Smart Detector supports
                    IList<ResourceIdentifier> resources = await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(requestResource.SubscriptionId, requestResource.ResourceGroupName, smartDetectorManifest.SupportedResourceTypes, cancellationToken);
                    resourcesForSmartDetector.UnionWith(resources);
                    this.tracer.TraceInformation($"Added {resources.Count} resources found in the specified resource group in subscription {requestResource.SubscriptionId}");
                }
                else
                {
                    // The Smart Detector does not support the requested resource type
                    throw new IncompatibleResourceTypesException(requestResource.ResourceType, smartDetectorManifest);
                }
            }

            return resourcesForSmartDetector.ToList();
        }
    }
}