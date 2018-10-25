//-----------------------------------------------------------------------
// <copyright file="TimeRangePickerControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for TimeRangePickerControl.xaml
    /// </summary>
    public partial class TimeRangePickerControl : UserControl
    {
        /// <summary>
        /// The control's minimum start date dependency property
        /// </summary>
        public static readonly DependencyProperty MinStartDateProperty = DependencyProperty.Register(
                                                                       "MinStartDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow.AddMonths(-3)));

        /// <summary>
        /// The control's selected start date dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedStartDateProperty = DependencyProperty.Register(
                                                                       "SelectedStartDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow,
                                                                            new PropertyChangedCallback(OnSelectedStartDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's selected start time dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedStartTimeProperty = DependencyProperty.Register(
                                                                       "SelectedStartTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow,
                                                                            new PropertyChangedCallback(OnSelectedStartDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's selected end date dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedEndDateProperty = DependencyProperty.Register(
                                                                       "SelectedEndDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow,
                                                                            new PropertyChangedCallback(OnSelectedEndDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's selected end time dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedEndTimeProperty = DependencyProperty.Register(
                                                                       "SelectedEndTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow,
                                                                            new PropertyChangedCallback(OnSelectedEndDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's maximum end date dependency property
        /// </summary>
        public static readonly DependencyProperty MaxEndDateProperty = DependencyProperty.Register(
                                                                       "MaxEndDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            DateTime.UtcNow));

        /// <summary>
        /// The control's full selected start date and time dependency property
        /// </summary>
        public static readonly DependencyProperty FullSelectedStartDateTimeProperty = DependencyProperty.Register(
                                                                       "FullSelectedStartDateTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl));

        /// <summary>
        /// The control's full selected end date and time dependency property
        /// </summary>
        public static readonly DependencyProperty FullSelectedEndDateTimeProperty = DependencyProperty.Register(
                                                                       "FullSelectedEndDateTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimeRangePickerControl));

        /// <summary>
        /// The control's hint text for selecting the date dependency property
        /// </summary>
        public static readonly DependencyProperty PickDateHintTextProperty = DependencyProperty.Register(
                                                                        "PickDateHintText",
                                                                        typeof(string),
                                                                        typeof(TimeRangePickerControl),
                                                                        new FrameworkPropertyMetadata(
                                                                            "Pick date"));

        /// <summary>
        /// The control's hint text for selecting the time dependency property
        /// </summary>
        public static readonly DependencyProperty PickTimeHintTextProperty = DependencyProperty.Register(
                                                                        "PickTimeHintText",
                                                                        typeof(string),
                                                                        typeof(TimeRangePickerControl),
                                                                        new FrameworkPropertyMetadata(
                                                                            "Select time (UTC)"));

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRangePickerControl"/> class.
        /// </summary>
        public TimeRangePickerControl()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the minimal start date of the range.
        /// </summary>
        public DateTime MinStartDate
        {
            get
            {
                return (DateTime)this.GetValue(MinStartDateProperty);
            }

            set
            {
                this.SetValue(MinStartDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected start date.
        /// </summary>
        public DateTime SelectedStartDate
        {
            get
            {
                return (DateTime)this.GetValue(SelectedStartDateProperty);
            }

            set
            {
                this.SetValue(SelectedStartDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected start time.
        /// </summary>
        public DateTime SelectedStartTime
        {
            get
            {
                return (DateTime)this.GetValue(SelectedStartTimeProperty);
            }

            set
            {
                this.SetValue(SelectedStartTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected end date.
        /// </summary>
        public DateTime SelectedEndDate
        {
            get
            {
                return (DateTime)this.GetValue(SelectedEndDateProperty);
            }

            set
            {
                this.SetValue(SelectedEndDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected end time.
        /// </summary>
        public DateTime SelectedEndTime
        {
            get
            {
                return (DateTime)this.GetValue(SelectedEndTimeProperty);
            }

            set
            {
                this.SetValue(SelectedEndTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximal end date of the range.
        /// </summary>
        public DateTime MaxEndDate
        {
            get
            {
                return (DateTime)this.GetValue(MaxEndDateProperty);
            }

            set
            {
                this.SetValue(MaxEndDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the full selected start date time.
        /// </summary>
        public DateTime FullSelectedStartDateTime
        {
            get
            {
                return (DateTime)this.GetValue(FullSelectedStartDateTimeProperty);
            }

            set
            {
                this.SetValue(FullSelectedStartDateTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the full selected end date time.
        /// </summary>
        public DateTime FullSelectedEndDateTime
        {
            get
            {
                return (DateTime)this.GetValue(FullSelectedEndDateTimeProperty);
            }

            set
            {
                this.SetValue(FullSelectedEndDateTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the hint text for date selection.
        /// </summary>
        public string PickDateHintText
        {
            get
            {
                return (string)this.GetValue(PickDateHintTextProperty);
            }

            set
            {
                this.SetValue(PickDateHintTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the hint text for time selection.
        /// </summary>
        public string PickTimeHintText
        {
            get
            {
                return (string)this.GetValue(PickTimeHintTextProperty);
            }

            set
            {
                this.SetValue(PickTimeHintTextProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// Updates <see cref="FullSelectedStartDateTime"/> according to start date or time selection change.
        /// </summary>
        /// <param name="d">the dependency object</param>
        /// <param name="e">the event args</param>
        private static void OnSelectedStartDateOrTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeRangePickerControl timeRangePickerControl = (TimeRangePickerControl)d;
            DateTime newSelectedTime = (DateTime)e.NewValue;

            if (e.Property == SelectedStartTimeProperty)
            {
                timeRangePickerControl.SelectedStartTime = newSelectedTime;

                if (timeRangePickerControl.SelectedStartDate.Date == timeRangePickerControl.SelectedEndDate.Date &&
                    newSelectedTime > timeRangePickerControl.SelectedEndTime)
                {
                    timeRangePickerControl.SelectedEndTime = newSelectedTime;
                }
            }

            if (e.Property == SelectedStartDateProperty &&
                timeRangePickerControl.SelectedStartDate.Date == timeRangePickerControl.SelectedEndDate.Date &&
                timeRangePickerControl.SelectedStartTime > timeRangePickerControl.SelectedEndTime)
            {
                timeRangePickerControl.SelectedStartTime = timeRangePickerControl.SelectedEndTime;
            }
        }

        /// <summary>
        /// Updates <see cref="FullSelectedEndDateTime"/> according to end date or time selection change.
        /// </summary>
        /// <param name="d">the dependency object</param>
        /// <param name="e">the event args</param>
        private static void OnSelectedEndDateOrTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeRangePickerControl timeRangePickerControl = (TimeRangePickerControl)d;
            DateTime newSelectedTime = (DateTime)e.NewValue;

            if (e.Property == SelectedEndTimeProperty)
            {
                timeRangePickerControl.SelectedEndTime = newSelectedTime;

                if (timeRangePickerControl.SelectedStartDate.Date == timeRangePickerControl.SelectedEndDate.Date &&
                    newSelectedTime < timeRangePickerControl.SelectedStartTime)
                {
                    timeRangePickerControl.SelectedStartTime = newSelectedTime;
                }
            }

            if (e.Property == SelectedEndDateProperty &&
                timeRangePickerControl.SelectedEndDate.Date == timeRangePickerControl.SelectedStartDate.Date &&
                timeRangePickerControl.SelectedStartTime > timeRangePickerControl.SelectedEndTime)
            {
                timeRangePickerControl.SelectedEndTime = timeRangePickerControl.SelectedStartTime;
            }
        }
    }
}
