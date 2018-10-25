//-----------------------------------------------------------------------
// <copyright file="ISystemProcessClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Interface for providing <see cref="Process"/> functionality, adjusted to emulator needs.
    /// Since emulator logic uses <see cref="Process"/>, this interface is necessary in order to mock it in unit tests.
    /// </summary>
    public interface ISystemProcessClient
    {
        /// <summary>
        /// Starts a new process using the web browser and run the given <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to run.</param>
        void StartWebBrowserProcess(Uri uri);
    }
}
