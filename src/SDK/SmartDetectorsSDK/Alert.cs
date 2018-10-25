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
        /// <param name="state">The alert's state. A Smart Detector can auto-resolve
        /// alerts that were created previously by passing <see cref="AlertState.Resolved"/> for this parameter.</param>
        protected Alert(string title, ResourceIdentifier resourceIdentifier, AlertState state = AlertState.Active)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (resourceIdentifier == null)
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }

            this.Title = title;
            this.ResourceIdentifier = resourceIdentifier;
            this.State = state;
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
        /// Gets the alert's state.
        /// </summary>
        public AlertState State { get; private set; }

        /// <summary>
        /// Sets the alert state to "Resolved".
        /// Note: for alert resolution to be registered in Azure Monitor - after the call to this method
        /// the alert object needs to be included in alerts returned by <see cref="ISmartDetector.AnalyzeResourcesAsync"/>
        /// method implementation in your Smart Detector.
        /// </summary>
        public void Resolve()
        {
            this.State = AlertState.Resolved;
        }
    }
}
