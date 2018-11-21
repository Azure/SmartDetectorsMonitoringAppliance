//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerChildProcessInput.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// Encapsulates the input for a Smart Detector Runner child process. This
    /// class contains all possible inputs for the different Smart Detector flows, and according
    /// to the values the child process can choose the right flow to run.
    /// </summary>
    public class SmartDetectorRunnerChildProcessInput
    {
        /// <summary>
        /// Gets or sets the Analysis request. This should contain a not <c>null</c> value
        /// if the child process should run the <see cref="ISmartDetectorRunner.AnalyzeAsync"/> flow.
        /// </summary>
        public SmartDetectorAnalysisRequest AnalysisRequest { get; set; }

        /// <summary>
        /// Gets or sets the Automatic Resolution Check request. This should contain a not <c>null</c> value
        /// if the child process should run the <see cref="ISmartDetectorRunner.CheckAutomaticResolutionAsync"/> flow.
        /// </summary>
        public AutomaticResolutionCheckRequest AutomaticResolutionCheckRequest { get; set; }
    }
}
