//-----------------------------------------------------------------------
// <copyright file="TracesControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
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
        private readonly IPageableLogArchive logArchive;
        private int pageSize;
        private IPageableLog pageableLog;
        private bool isSmartDetectorRunning;
        private ObservableTask loadLogTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracesControlViewModel"/> class.
        /// </summary>
        public TracesControlViewModel()
        {
            this.SupportedPageSizes = new List<int> { 50, 100, 150, 200 };
            this.pageSize = this.SupportedPageSizes[0];
            this.isSmartDetectorRunning = false;
            this.LoadLogTask = new ObservableTask(Task.FromResult(false), null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TracesControlViewModel"/> class.
        /// </summary>
        /// <param name="smartDetectorRunner">The Smart Detector runner to get the log from.</param>
        /// <param name="logArchive">The pageable log archive.</param>
        /// <param name="tracer">The tracer to use for tracing observable tasks.</param>
        [InjectionConstructor]
        public TracesControlViewModel(IEmulationSmartDetectorRunner smartDetectorRunner, IPageableLogArchive logArchive, ITracer tracer)
            : this()
        {
            this.tracer = tracer;
            this.logArchive = logArchive;

            smartDetectorRunner.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(smartDetectorRunner.PageableLog))
                {
                    this.PageableLog = smartDetectorRunner.PageableLog;
                }
                else if (args.PropertyName == nameof(smartDetectorRunner.IsSmartDetectorRunning))
                {
                    this.IsSmartDetectorRunning = smartDetectorRunner.IsSmartDetectorRunning;
                }
            };
        }

        /// <summary>
        /// Gets the list of supported page sizes.
        /// </summary>
        public List<int> SupportedPageSizes { get; }

        /// <summary>
        /// Gets or sets the pageable log to display.
        /// </summary>
        public IPageableLog PageableLog
        {
            get => this.pageableLog;
            set
            {
                // Unregister from the old log
                if (this.pageableLog != null)
                {
                    this.pageableLog.PropertyChanged -= this.PageableLogOnPropertyChanged;
                }

                // Set the log, and register for notifications
                this.pageableLog = value;
                if (this.pageableLog != null)
                {
                    // Make sure the log's page size matches the last user selection
                    value.PageSize = this.PageSize;
                    this.pageableLog.PropertyChanged += this.PageableLogOnPropertyChanged;
                }

                // Finally - fire a property changed event with empty name, this will force refresh of all bindings in the model
                this.OnPropertyChanged(string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the detector is currently running.
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
        /// Gets or sets the name of the currently displayed log.
        /// </summary>
        public string CurrentLogName
        {
            get => this.PageableLog?.Name ?? string.Empty;
            set => this.LoadLogTask = new ObservableTask(this.LoadLogAsync(value), this.tracer);
        }

        /// <summary>
        /// Gets a task for tracking a log's load operation
        /// </summary>
        public ObservableTask LoadLogTask
        {
            get => this.loadLogTask;
            private set
            {
                this.loadLogTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the log names available in the log archive
        /// </summary>
        public ObservableCollection<string> LogNames => this.logArchive.LogNames;

        /// <summary>
        /// Gets or sets the current traces pages size.
        /// </summary>
        public int PageSize
        {
            get => this.pageSize;
            set
            {
                this.pageSize = value;
                if (this.PageableLog != null)
                {
                    this.PageableLog.PageSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current page's 1-based index.
        /// </summary>
        public int CurrentPageIndex
        {
            get => (this.PageableLog?.CurrentPageIndex + 1) ?? 0;
            set
            {
                if (this.PageableLog == null)
                {
                    // Don't allow any updates, so just ignore the value and notify so the UI will be updated
                    this.OnPropertyChanged();
                }
                else
                {
                    // Make sure to stay in bounds
                    if (value <= 0)
                    {
                        value = 1;
                    }
                    else if (value > this.pageableLog.NumberOfPages)
                    {
                        value = this.pageableLog.NumberOfPages;
                    }

                    this.PageableLog.CurrentPageIndex = value - 1;
                    this.OnPropertyChanged(nameof(this.IsFirstPage));
                    this.OnPropertyChanged(nameof(this.IsLastPage));
                }
            }
        }

        /// <summary>
        /// Gets the 1-based index of the current page's first trace line.
        /// </summary>
        public int CurrentPageStart => (this.PageableLog?.CurrentPageStart + 1) ?? 0;

        /// <summary>
        /// Gets the 1-based index of the current page's last trace line.
        /// </summary>
        public int CurrentPageEnd => (this.PageableLog?.CurrentPageEnd + 1) ?? 0;

        /// <summary>
        /// Gets the total number of trace pages.
        /// </summary>
        public int NumberOfPages => this.PageableLog?.NumberOfPages ?? 0;

        /// <summary>
        /// Gets the total number of trace lines in the log.
        /// </summary>
        public int NumberOfTraceLines => this.PageableLog?.NumberOfTraceLines ?? 0;

        /// <summary>
        /// Gets a value indicating whether we are currently showing the first page.
        /// </summary>
        public bool IsFirstPage => this.CurrentPageIndex <= 1;

        /// <summary>
        /// Gets a value indicating whether we are currently showing the last page.
        /// </summary>
        public bool IsLastPage => this.CurrentPageIndex == this.NumberOfPages;

        /// <summary>
        /// Gets a command for moving to the first log page.
        /// </summary>
        public CommandHandler FirstPageCommand => new CommandHandler(() => this.CurrentPageIndex = 1);

        /// <summary>
        /// Gets a command for moving to the previous log page.
        /// </summary>
        public CommandHandler PrevPageCommand => new CommandHandler(() => this.CurrentPageIndex--);

        /// <summary>
        /// Gets a command for moving to the next log page.
        /// </summary>
        public CommandHandler NextPageCommand => new CommandHandler(() => this.CurrentPageIndex++);

        /// <summary>
        /// Gets a command for moving to the last log page.
        /// </summary>
        public CommandHandler LastPageCommand => new CommandHandler(() => this.CurrentPageIndex = this.NumberOfPages);

        /// <summary>
        /// Handler for property changed events on <see cref="PageableLog"/>.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments</param>
        private void PageableLogOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Send the notification as if this object has changed - we match the property names, so it should work
            this.OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName)
            {
                case nameof(this.CurrentPageIndex):
                    this.OnPropertyChanged(nameof(this.IsFirstPage));
                    this.OnPropertyChanged(nameof(this.IsLastPage));
                    break;

                case nameof(this.NumberOfPages):
                    this.OnPropertyChanged(nameof(this.IsLastPage));
                    break;
            }
        }

        /// <summary>
        /// Loads the log with the specified name to the view model
        /// </summary>
        /// <param name="logName">The name of the log to load</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        private async Task LoadLogAsync(string logName)
        {
            this.PageableLog = null;
            this.PageableLog = await this.logArchive.GetLogAsync(logName, this.PageSize);
        }
    }
}
