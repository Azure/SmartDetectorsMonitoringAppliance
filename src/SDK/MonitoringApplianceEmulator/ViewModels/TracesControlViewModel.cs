//-----------------------------------------------------------------------
// <copyright file="TracesControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Unity.Attributes;

    /// <summary>
    /// The view model for the traces control.
    /// </summary>
    public class TracesControlViewModel : ObservableObject
    {
        private readonly ITracer tracer;

        private int pageSize;
        private IPageableLogTracer pageableTracer;
        private ObservableTask updatePageTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracesControlViewModel"/> class.
        /// </summary>
        public TracesControlViewModel()
        {
            this.SupportedPageSizes = new List<int> { 50, 100, 150, 200 };
            this.pageSize = this.SupportedPageSizes[0];
            this.UpdatePageTask = new ObservableTask(Task.FromResult(true), null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TracesControlViewModel"/> class.
        /// </summary>
        /// <param name="smartDetectorRunner">The Smart Detector runner to get the tracer from.</param>
        /// <param name="tracer">The tracer to use.</param>
        [InjectionConstructor]
        public TracesControlViewModel(IEmulationSmartDetectorRunner smartDetectorRunner, ITracer tracer)
            : this()
        {
            this.tracer = tracer;
            this.UpdatePageTask = new ObservableTask(Task.FromResult(true), this.tracer);
            smartDetectorRunner.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(smartDetectorRunner.PageableLogTracer))
                {
                    this.PageableTracer = smartDetectorRunner.PageableLogTracer;
                }
            };
        }

        /// <summary>
        /// Gets the list of supported page sizes.
        /// </summary>
        public List<int> SupportedPageSizes { get; }

        /// <summary>
        /// Gets or sets the pageable log tracer to display.
        /// </summary>
        public IPageableLogTracer PageableTracer
        {
            get => this.pageableTracer;
            set
            {
                // Unregister from the old tracer
                if (this.pageableTracer != null)
                {
                    this.pageableTracer.PropertyChanged -= this.PageableTracerOnPropertyChanged;
                }

                // Set the tracer, and re-apply the page size
                this.pageableTracer = value;
                if (this.pageableTracer != null)
                {
                    this.pageableTracer.PropertyChanged += this.PageableTracerOnPropertyChanged;
                }

                this.OnPropertyChanged();
                this.PageSize = this.pageSize;
            }
        }

        /// <summary>
        /// Gets or sets the current traces pages size.
        /// </summary>
        public int PageSize
        {
            get => this.pageSize;
            set
            {
                this.pageSize = value;
                if (this.PageableTracer != null)
                {
                    this.UpdatePageTask = new ObservableTask(this.PageableTracer.SetPageSizeAsync(value), this.tracer, this.OnPageUpdated);
                }
                else
                {
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current page's 1-based index.
        /// </summary>
        public int CurrentPageIndex
        {
            get => (this.PageableTracer?.CurrentPageIndex + 1) ?? 0;
            set
            {
                if (this.PageableTracer == null)
                {
                    // Don't allow any updates, so just ignore the value and notify so the UI will be updated
                    this.OnPropertyChanged();
                }
                else
                {
                    if (value <= 0 || value > this.pageableTracer.NumberOfPages)
                    {
                        // Invalid value, so ignore the value and notify so the UI will be updated
                        this.OnPropertyChanged();
                    }
                    else
                    {
                        this.UpdatePageTask = new ObservableTask(this.PageableTracer.SetCurrentPageIndexAsync(value - 1), this.tracer, this.OnPageUpdated);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the 1-based index of the current page's first trace line.
        /// </summary>
        public int CurrentPageStart => (this.PageableTracer?.CurrentPageStart + 1) ?? 0;

        /// <summary>
        /// Gets the 1-based index of the current page's last trace line.
        /// </summary>
        public int CurrentPageEnd => (this.PageableTracer?.CurrentPageEnd + 1) ?? 0;

        /// <summary>
        /// Gets the total number of trace pages.
        /// </summary>
        public int NumberOfPages => this.PageableTracer?.NumberOfPages ?? 0;

        /// <summary>
        /// Gets the total number of trace lines in the log.
        /// </summary>
        public int NumberOfTraceLines => this.PageableTracer?.NumberOfTraceLines ?? 0;

        /// <summary>
        /// Gets a value indicating whether we are currently showing the first page.
        /// </summary>
        public bool IsFirstPage => this.CurrentPageIndex <= 1;

        /// <summary>
        /// Gets a value indicating whether we are currently showing the last page.
        /// </summary>
        public bool IsLastPage => this.CurrentPageIndex == this.NumberOfPages;

        /// <summary>
        /// Gets or sets the task for updating the traces page
        /// </summary>
        public ObservableTask UpdatePageTask
        {
            get => this.updatePageTask;

            set
            {
                this.updatePageTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a command for moving to the first log page.
        /// </summary>
        public CommandHandler FirstPageCommand => new CommandHandler(
            () => this.updatePageTask = new ObservableTask(this.PageableTracer.SetCurrentPageIndexAsync(0), this.tracer));

        /// <summary>
        /// Gets a command for moving to the previous log page.
        /// </summary>
        public CommandHandler PrevPageCommand => new CommandHandler(
            () => this.updatePageTask = new ObservableTask(this.PageableTracer.SetCurrentPageIndexAsync(this.CurrentPageIndex - 1), this.tracer));

        /// <summary>
        /// Gets a command for moving to the next log page.
        /// </summary>
        public CommandHandler NextPageCommand => new CommandHandler(
            () => this.updatePageTask = new ObservableTask(this.PageableTracer.SetCurrentPageIndexAsync(this.CurrentPageIndex + 1), this.tracer));

        /// <summary>
        /// Gets a command for moving to the last log page.
        /// </summary>
        public CommandHandler LastPageCommand => new CommandHandler(
            () => this.updatePageTask = new ObservableTask(this.PageableTracer.SetCurrentPageIndexAsync(this.NumberOfPages - 1), this.tracer));

        /// <summary>
        /// Callback for handling the completion of the page update. Basically notifies that everything has changed.
        /// </summary>
        private void OnPageUpdated()
        {
            this.OnPropertyChanged(nameof(this.PageSize));
            this.OnPropertyChanged(nameof(this.CurrentPageIndex));
            this.OnPropertyChanged(nameof(this.CurrentPageStart));
            this.OnPropertyChanged(nameof(this.CurrentPageEnd));
            this.OnPropertyChanged(nameof(this.NumberOfPages));
            this.OnPropertyChanged(nameof(this.IsFirstPage));
            this.OnPropertyChanged(nameof(this.IsLastPage));
        }

        /// <summary>
        /// Handler for property changed events on <see cref="PageableTracer"/>.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments</param>
        private void PageableTracerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Send the notification as if this object has changed - we match the property names, so it should work
            this.OnPropertyChanged(e.PropertyName);
        }
    }
}
