﻿//-----------------------------------------------------------------------
// <copyright file="AnalysisRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;

    /// <summary>
    /// Represents a single analysis request sent to the Smart Detector. This is the main parameter sent to the
    /// <see cref="ISmartDetector.AnalyzeResourcesAsync"/> method.
    /// </summary>
    public class AnalysisRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisRequest"/> class.
        /// </summary>
        /// <param name="requestParameters">The request parameters as received from the Azure Monitoring back-end.</param>
        /// <param name="analysisServicesFactory">The analysis services factory to be used for querying the resources telemetry.</param>
        /// <param name="stateRepository">The persistent state repository for storing state between analysis runs</param>
        public AnalysisRequest(
            AnalysisRequestParameters requestParameters,
            IAnalysisServicesFactory analysisServicesFactory,
            IStateRepository stateRepository)
        {
            if (requestParameters == null)
            {
                throw new ArgumentNullException(nameof(requestParameters));
            }

            if (analysisServicesFactory == null)
            {
                throw new ArgumentNullException(nameof(analysisServicesFactory));
            }

            this.RequestParameters = requestParameters;
            this.AnalysisServicesFactory = analysisServicesFactory;
            this.StateRepository = stateRepository;
        }

        /// <summary>
        /// Gets the request parameters as received from the Azure Monitoring back-end.
        /// </summary>
        public AnalysisRequestParameters RequestParameters { get; }

        /// <summary>
        /// Gets the analysis services factory to be used for querying the resources telemetry.
        /// </summary>
        public IAnalysisServicesFactory AnalysisServicesFactory { get; }

        /// <summary>
        /// Gets the persistent state repository for storing state between analysis runs.
        /// </summary>
        public IStateRepository StateRepository { get; }
    }
}
