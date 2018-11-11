//-----------------------------------------------------------------------
// <copyright file="IEmulationSmartDetectorRunner.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;

    /// <summary>
    /// An interface for running a Smart Detector within the emulator.
    /// </summary>
    public interface IEmulationSmartDetectorRunner : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the Smart Detector run's alerts.
        /// </summary>
        ObservableCollection<EmulationAlert> Alerts { get; }

        /// <summary>
        /// Gets a value indicating whether the Smart Detector is running.
        /// </summary>
        bool IsSmartDetectorRunning { get; }

        /// <summary>
        /// Gets the log used for the last (or current) run.
        /// </summary>
        IPageableLog PageableLog { get; }

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