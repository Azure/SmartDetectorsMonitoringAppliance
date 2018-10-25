//-----------------------------------------------------------------------
// <copyright file="SystemProcessClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Provides <see cref="Process"/> functionality, adjusted to emulator needs.
    /// </summary>
    public class SystemProcessClient : ISystemProcessClient
    {
        /// <summary>
        /// Starts a new process using the web browser in order to run the given <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to run.</param>
        public void StartWebBrowserProcess(Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }
    }
}
