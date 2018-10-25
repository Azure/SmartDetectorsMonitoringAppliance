//-----------------------------------------------------------------------
// <copyright file="IEmulationSmartDetectorRunner.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for running a Smart Detector within the emulator.
    /// </summary>
    public interface IEmulationSmartDetectorRunner
    {
        /// <summary>
        /// Runs the Smart Detector asynchronously.
        /// </summary>
        /// <param name="targetResource">The resource which the Smart Detector should run on.</param>
        /// <param name="allResources">All supported resources in subscription.</param>
        /// <param name="analysisCadence">The analysis cadence.</param>
        /// <param name="startTimeRange">The start time.</param>
        /// <param name="endTimeRange">The end time.</param>
        /// <returns>A task that runs the Smart Detector.</returns>
        Task RunAsync(HierarchicalResource targetResource, List<ResourceIdentifier> allResources, TimeSpan analysisCadence, DateTime startTimeRange, DateTime endTimeRange);

        /// <summary>
        /// Cancels the Smart Detector run.
        /// </summary>
        void CancelSmartDetectorRun();
    }
}