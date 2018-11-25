//-----------------------------------------------------------------------
// <copyright file="TimePickerControl.xaml.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for TimePickerControl.xaml
    /// </summary>
    public partial class TimePickerControl : UserControl
    {
        /// <summary>
        /// The control's title dependency property
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
                                                                       "Title",
                                                                       typeof(string),
                                                                       typeof(TimePickerControl));

        /// <summary>
        /// The control's minimum date dependency property
        /// </summary>
        public static readonly DependencyProperty MinDateProperty = DependencyProperty.Register(
                                                                       "MinDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimePickerControl));

        /// <summary>
        /// The control's maximum date dependency property
        /// </summary>
        public static readonly DependencyProperty MaxDateProperty = DependencyProperty.Register(
                                                                       "MaxDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimePickerControl));

        /// <summary>
        /// The control's selected date dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
                                                                       "SelectedDate",
                                                                       typeof(DateTime),
                                                                       typeof(TimePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            new PropertyChangedCallback(OnSelectedDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's selected time dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
                                                                       "SelectedTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                            new PropertyChangedCallback(OnSelectedDateOrTimePropertyChanged)));

        /// <summary>
        /// The control's full selected time dependency property
        /// </summary>
        public static readonly DependencyProperty FullSelectedDateTimeProperty = DependencyProperty.Register(
                                                                       "FullSelectedDateTime",
                                                                       typeof(DateTime),
                                                                       typeof(TimePickerControl),
                                                                       new FrameworkPropertyMetadata(
                                                                           new PropertyChangedCallback(OnFullSelectedDateTimePropertyChanged)));

        /// <summary>
        /// The control's hint text for selecting the date dependency property
        /// </summary>
        public static readonly DependencyProperty PickDateHintTextProperty = DependencyProperty.Register(
                                                                        "PickDateHintText",
                                                                        typeof(string),
                                                                        typeof(TimePickerControl));

        /// <summary>
        /// The control's hint text for selecting the time dependency property
        /// </summary>
        public static readonly DependencyProperty PickTimeHintTextProperty = DependencyProperty.Register(
                                                                        "PickTimeHintText",
                                                                        typeof(string),
                                                                        typeof(TimePickerControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePickerControl"/> class.
        /// </summary>
        public TimePickerControl()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)this.GetValue(TitleProperty);
            }

            set
            {
                this.SetValue(TitleProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimal date that can be selected by the user.
        /// </summary>
        public DateTime MinDate
        {
            get
            {
                return (DateTime)this.GetValue(MinDateProperty);
            }

            set
            {
                this.SetValue(MinDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximal date that can be selected by the user.
        /// </summary>
        public DateTime MaxDate
        {
            get
            {
                return (DateTime)this.GetValue(MaxDateProperty);
            }

            set
            {
                this.SetValue(MaxDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        public DateTime SelectedDate
        {
            get
            {
                return (DateTime)this.GetValue(SelectedDateProperty);
            }

            set
            {
                this.SetValue(SelectedDateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected time.
        /// </summary>
        public DateTime SelectedTime
        {
            get
            {
                return (DateTime)this.GetValue(SelectedTimeProperty);
            }

            set
            {
                this.SetValue(SelectedTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the full selected date time.
        /// </summary>
        public DateTime FullSelectedDateTime
        {
            get
            {
                return (DateTime)this.GetValue(FullSelectedDateTimeProperty);
            }

            set
            {
                this.SetValue(FullSelectedDateTimeProperty, value);
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
        /// Updates <see cref="FullSelectedDateTime"/> according to date or time selection change.
        /// </summary>
        /// <param name="d">the dependency object</param>
        /// <param name="e">the event args</param>
        private static void OnSelectedDateOrTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimePickerControl timePickerControl = (TimePickerControl)d;
            DateTime newSelectedTime = (DateTime)e.NewValue;

            timePickerControl.FullSelectedDateTime = new DateTime(
                timePickerControl.SelectedDate.Year,
                timePickerControl.SelectedDate.Month,
                timePickerControl.SelectedDate.Day,
                timePickerControl.SelectedTime.Hour,
                timePickerControl.SelectedTime.Minute,
                second: 0,
                kind: DateTimeKind.Utc);
        }

        /// <summary>
        /// Updates <see cref="SelectedDate"/> and <see cref="SelectedTime"/> according to the full selected date time change.
        /// </summary>
        /// <param name="d">the dependency object</param>
        /// <param name="e">the event args</param>
        private static void OnFullSelectedDateTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimePickerControl timePickerControl = (TimePickerControl)d;
            DateTime newSelectedTime = (DateTime)e.NewValue;

            timePickerControl.SelectedDate = new DateTime(
                timePickerControl.SelectedDate.Year,
                timePickerControl.SelectedDate.Month,
                timePickerControl.SelectedDate.Day);

            timePickerControl.SelectedTime = newSelectedTime;
        }
    }
}