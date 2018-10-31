//-----------------------------------------------------------------------
// <copyright file="IPageableLogTracer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for a pageable log tracer
    /// </summary>
    public interface IPageableLogTracer : ITracer, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the total number of trace lines in the log.
        /// </summary>
        int NumberOfTraceLines { get; }

        /// <summary>
        /// Gets the page size (number of lines in a single page).
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets the total number of pages in the log - this can change when new trace
        /// lines are added, or when <see cref="PageSize"/> is changed.
        /// </summary>
        int NumberOfPages { get; }

        /// <summary>
        /// Gets the current page index.
        /// </summary>
        int CurrentPageIndex { get; }

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
        /// Sets the log's page size to be <paramref name="pageSize"/>. Calling this method will
        /// have the effect of an update to <see cref="CurrentPageTraces"/> and possibly to <see cref="CurrentPageIndex"/>.
        /// </summary>
        /// <param name="pageSize">The updated page size.</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        Task SetPageSizeAsync(int pageSize);

        /// <summary>
        /// Moves <see cref="CurrentPageIndex"/> to be <paramref name="pageIndex"/>. If <paramref name="pageIndex"/> is outside
        /// of the log's page range, calling this method will not have any affect.
        /// </summary>
        /// <param name="pageIndex">The page index to set.</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        Task SetCurrentPageIndexAsync(int pageIndex);
    }
}
