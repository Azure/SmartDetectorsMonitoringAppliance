﻿//-----------------------------------------------------------------------
// <copyright file="SmartSignalRunner.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.DetectionPresentation;

    /// <summary>
    /// An implementation of <see cref="ISmartSignalRunner"/>, that loads the signal and runs it
    /// </summary>
    public class SmartSignalRunner : ISmartSignalRunner
    {
        private readonly ISmartSignalsRepository smartSignalsRepository;
        private readonly ISmartSignalLoader smartSignalLoader;
        private readonly IAnalysisServicesFactory analysisServicesFactory;
        private readonly IAzureResourceManagerClient azureResourceManagerClient;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRunner"/> class
        /// </summary>
        /// <param name="smartSignalsRepository">The smart signals repository</param>
        /// <param name="smartSignalLoader">The smart signals loader</param>
        /// <param name="analysisServicesFactory">The analysis services factory</param>
        /// <param name="azureResourceManagerClient">The azure resource manager client</param>
        /// <param name="tracer">The tracer</param>
        public SmartSignalRunner(
            ISmartSignalsRepository smartSignalsRepository,
            ISmartSignalLoader smartSignalLoader,
            IAnalysisServicesFactory analysisServicesFactory,
            IAzureResourceManagerClient azureResourceManagerClient,
            ITracer tracer)
        {
            this.smartSignalsRepository = Diagnostics.EnsureArgumentNotNull(() => smartSignalsRepository);
            this.smartSignalLoader = Diagnostics.EnsureArgumentNotNull(() => smartSignalLoader);
            this.analysisServicesFactory = Diagnostics.EnsureArgumentNotNull(() => analysisServicesFactory);
            this.azureResourceManagerClient = Diagnostics.EnsureArgumentNotNull(() => azureResourceManagerClient);
            this.tracer = tracer;
        }

        #region Implementation of ISmartSignalRunner

        /// <summary>
        /// Loads the signal, runs it, and returns the generated detections
        /// </summary>
        /// <param name="request">The signal request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the detections generated by the signal</returns>
        public async Task<List<SmartSignalDetectionPresentation>> RunAsync(SmartSignalRequest request, CancellationToken cancellationToken)
        {
            // Read the signal's metadata
            this.tracer.TraceInformation($"Loading signal metadata for signal ID {request.SignalId}");
            SmartSignalMetadata signalMetadata = await this.smartSignalsRepository.ReadSignalMetadataAsync(request.SignalId);
            this.tracer.TraceInformation($"Read signal metadata, ID {signalMetadata.Id}, Version {signalMetadata.Version}");

            // Load the signal
            ISmartSignal signal = await this.smartSignalLoader.LoadSignalAsync(signalMetadata);
            this.tracer.TraceInformation($"Signal instance created successfully, ID {signalMetadata.Id}");

            // Determine the analysis window
            TimeRange analysisWindow = new TimeRange(request.AnalysisStartTime, request.AnalysisEndTime);
            this.tracer.TraceInformation($"Signal analysis window is: {analysisWindow}");

            // Get the resources on which to run the signal
            List<ResourceIdentifier> resources = await this.GetResourcesForSignal(request.ResourceIds, signalMetadata, cancellationToken);

            // Run the signal
            this.tracer.TraceInformation($"Started running signal ID {signalMetadata.Id}, Name {signalMetadata.Name}");
            List<SmartSignalDetection> detections;
            try
            {
                detections = await signal.AnalyzeResourcesAsync(resources, analysisWindow, this.analysisServicesFactory, this.tracer, cancellationToken);
                this.tracer.TraceInformation($"Completed running signal ID {signalMetadata.Id}, Name {signalMetadata.Name}, returning {detections.Count} detections");
            }
            catch (Exception e)
            {
                this.tracer.TraceInformation($"Failed running signal ID {signalMetadata.Id}, Name {signalMetadata.Name}: {e.Message}");
                throw;
            }

            // Verify that each detection belongs to one of the provided resources
            foreach (SmartSignalDetection detection in detections)
            {
                if (resources.All(resource => resource != detection.ResourceIdentifier))
                {
                    throw new UnidentifiedDetectionResourceException(detection.ResourceIdentifier);
                }
            }

            // Trace the number of detections of each type
            foreach (var typeDetections in detections.GroupBy(x => x.GetType().Name))
            {
                this.tracer.TraceInformation($"Got {typeDetections.Count()} detections of type '{typeDetections.Key}'");
                this.tracer.ReportMetric("SignalDetectionType", typeDetections.Count(), new Dictionary<string, string>() { { "DetectionType", typeDetections.Key } });
            }

            // And return the detections
            return detections.Select(detection => SmartSignalDetectionPresentation.CreateFromDetection(request, signalMetadata.Name, detection, this.azureResourceManagerClient)).ToList();
        }

        #endregion

        /// <summary>
        /// Verify that the request resource type is supported by the signal, and enumerate
        /// the resources that the signal should run on.
        /// </summary>
        /// <param name="requestResourceIds">The request resource Ids</param>
        /// <param name="smartSignalMetadata">The signal metadata</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the resource identifiers that the signal should run on</returns>
        private async Task<List<ResourceIdentifier>> GetResourcesForSignal(IList<string> requestResourceIds, SmartSignalMetadata smartSignalMetadata, CancellationToken cancellationToken)
        {
            HashSet<ResourceIdentifier> resourcesForSignal = new HashSet<ResourceIdentifier>();
            foreach (string requestResourceId in requestResourceIds)
            {
                ResourceIdentifier requestResource = this.azureResourceManagerClient.GetResourceIdentifier(requestResourceId);

                if (smartSignalMetadata.SupportedResourceTypes.Contains(requestResource.ResourceType))
                {
                    // If the signal directly supports the requested resource type, then that's it
                    resourcesForSignal.Add(requestResource);
                }
                else if (requestResource.ResourceType == ResourceType.Subscription && smartSignalMetadata.SupportedResourceTypes.Contains(ResourceType.ResourceGroup))
                {
                    // If the request is for a subscription, and the signal supports a resource group type, enumerate all resource groups in the requested subscription
                    IList<ResourceIdentifier> resourceGroups = await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(requestResource.SubscriptionId, cancellationToken);
                    resourcesForSignal.UnionWith(resourceGroups);
                    this.tracer.TraceInformation($"Added {resourceGroups.Count} resource groups found in subscription {requestResource.SubscriptionId}");
                }
                else if (requestResource.ResourceType == ResourceType.Subscription)
                {
                    // If the request is for a subscription, enumerate all the resources in the requested subscription that the signal supports
                    IList<ResourceIdentifier> resources = await this.azureResourceManagerClient.GetAllResourcesInSubscriptionAsync(requestResource.SubscriptionId, smartSignalMetadata.SupportedResourceTypes, cancellationToken);
                    resourcesForSignal.UnionWith(resources);
                    this.tracer.TraceInformation($"Added {resources.Count} resources found in subscription {requestResource.SubscriptionId}");
                }
                else if (requestResource.ResourceType == ResourceType.ResourceGroup && smartSignalMetadata.SupportedResourceTypes.Any(type => type != ResourceType.Subscription))
                {
                    // If the request is for a resource group, and the signal supports resource types (other than subscription),
                    // enumerate all the resources in the requested resource group that the signal supports
                    IList<ResourceIdentifier> resources = await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(requestResource.SubscriptionId, requestResource.ResourceGroupName, smartSignalMetadata.SupportedResourceTypes, cancellationToken);
                    resourcesForSignal.UnionWith(resources);
                    this.tracer.TraceInformation($"Added {resources.Count} resources found in the specified resource group in subscription {requestResource.SubscriptionId}");
                }
                else
                {
                    // The signal does not support the requested resource type
                    throw new IncompatibleResourceTypesException(requestResource.ResourceType, smartSignalMetadata);
                }
            }

            return resourcesForSignal.ToList();
        }
    }
}