//-----------------------------------------------------------------------
// <copyright file="IPageableLogArchiveFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    /// <summary>
    /// An interface for a factory for generating <see cref="IPageableLogArchive"/> instances
    /// </summary>
    public interface IPageableLogArchiveFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IPageableLogArchive"/> interface.
        /// </summary>
        /// <returns>The newly created instance</returns>
        IPageableLogArchive Create();
    }
}
