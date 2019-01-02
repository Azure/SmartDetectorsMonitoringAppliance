//-----------------------------------------------------------------------
// <copyright file="Alert.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;

    /// <summary>
    /// The base class for representing a specific Alert.
    /// Each Alert instance contains both the detected issue's data and representation properties.
    /// </summary>
    public abstract class Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        protected Alert(string title, ResourceIdentifier resourceIdentifier)
            : this(title, resourceIdentifier, ExtendedDateTime.UtcNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        /// <param name="occurenceTime">The exact time at which the issue that caused the alert has occured. If this is a continuous issue - pass the issue start time.</param>
        protected Alert(string title, ResourceIdentifier resourceIdentifier, DateTime occurenceTime)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            this.Title = title;
            this.ResourceIdentifier = resourceIdentifier;
            this.OccurenceTime = occurenceTime;
        }

        /// <summary>
        /// Gets the title of this Alert.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the resource identifier that this Alert applies to.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier { get; }

        /// <summary>
        /// Gets the exact time at which the issue that caused the alert has occured or an issue start time in case of a continuous issue.
        /// </summary>
        public DateTime OccurenceTime { get; }

        /// <summary>
        /// Gets or sets optional resolution parameters for this Alert.
        /// A Smart Detector can use this property to signal to the Azure Monitoring back-end the conditions
        /// under which the Alert can be resolved (without user interaction). A Smart Detector which
        /// provides this property must implement the <see cref="IResolvableAlertSmartDetector"/> interface
        /// in order to handle resolution check requests sent by the Azure Monitoring back-end.
        /// </summary>
        public AlertResolutionParameters AlertResolutionParameters { get; set; }
    }
}
