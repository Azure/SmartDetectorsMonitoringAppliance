//-----------------------------------------------------------------------
// <copyright file="IPageableLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// An interface for a pageable log
    /// </summary>
    public interface IPageableLog : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the log name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the total number of trace lines in the log.
        /// </summary>
        int NumberOfTraceLines { get; }

        /// <summary>
        /// Gets or sets the page size (number of lines in a single page).
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of pages in the log - this can change when new trace
        /// lines are added, or when <see cref="PageSize"/> is changed.
        /// </summary>
        int NumberOfPages { get; }

        /// <summary>
        /// Gets or sets the current page index.
        /// </summary>
        int CurrentPageIndex { get; set; }

        /// <summary>
        /// Gets the index of the current page's first trace line.
        /// </summary>
        int CurrentPageStart { get; }

        /// <summary>
        /// Gets the index of the current page's last trace line.
        /// </summary>
        int CurrentPageEnd { get; }

        /// <summary>
        /// Gets the traces of the current page
        /// </summary>
        ObservableCollection<TraceLine> CurrentPageTraces { get; }

        /// <summary>
        /// Creates a tracer that sends trace lines to the log.
        /// </summary>
        /// <returns>A tracer that sends trace lines to the log.</returns>
        ILogArchiveTracer CreateTracer();
    }
}
