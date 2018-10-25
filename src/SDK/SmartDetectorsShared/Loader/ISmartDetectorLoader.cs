//-----------------------------------------------------------------------
// <copyright file="ISmartDetectorLoader.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Loader
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// An interface used for loading a Smart Detector from its package
    /// </summary>
    public interface ISmartDetectorLoader
    {
        /// <summary>
        /// Load a Smart Detector from its package.
        /// </summary>
        /// <param name="smartDetectorPackage">The Smart Detector package.</param>
        /// <returns>The Smart Detector instance.</returns>
        ISmartDetector LoadSmartDetector(SmartDetectorPackage smartDetectorPackage);
    }
}