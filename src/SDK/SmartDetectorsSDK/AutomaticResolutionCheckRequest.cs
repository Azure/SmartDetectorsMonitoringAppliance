//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionCheckRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;

    /// <summary>
    /// Represents a request sent to the Smart Detector to check if a previously fired alert can be automatically resolved.
    /// </summary>
    public class AutomaticResolutionCheckRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionCheckRequest"/> class.
        /// </summary>
        /// <param name="originalAnalysisRequestParameters">The original analysis request parameters as received from the Azure Monitoring back-end.</param>
        /// <param name="requestParameters">The request parameters as received from the Azure Monitoring back-end.</param>
        /// <param name="analysisServicesFactory">The analysis services factory to be used for querying the resources telemetry.</param>
        /// <param name="stateRepository">The persistent state repository for storing state between analysis runs</param>
        public AutomaticResolutionCheckRequest(
            AnalysisRequestParameters originalAnalysisRequestParameters,
            AutomaticResolutionCheckRequestParameters requestParameters,
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

            this.OriginalAnalysisRequestParameters = originalAnalysisRequestParameters;
            this.RequestParameters = requestParameters;
            this.AnalysisServicesFactory = analysisServicesFactory;
            this.StateRepository = stateRepository;
        }

        /// <summary>
        /// Gets the original analysis request parameters as received from the Azure Monitoring back-end.
        /// </summary>
        public AnalysisRequestParameters OriginalAnalysisRequestParameters { get; }

        /// <summary>
        /// Gets the request parameters as received from the Azure Monitoring back-end.
        /// </summary>
        public AutomaticResolutionCheckRequestParameters RequestParameters { get; }

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
