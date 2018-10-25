//-----------------------------------------------------------------------
// <copyright file="PackageSmartDetector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Build
{
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Represents the build task of a Smart Detector
    /// </summary>
    public class PackageSmartDetector : Task
    {
        /// <summary>
        /// Gets or sets the path of the zipped package.
        /// </summary>
        public string PackagePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the zipped package.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Executes the Smart Detector build process.
        /// </summary>
        /// <returns>True if the task successfully executed; otherwise, False.</returns>
        public override bool Execute()
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
                {
                    Arguments = $"{this.PackagePath} {this.PackageName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    this.Log.LogError(process.StandardOutput.ReadToEnd());
                    return false;
                }

                return true;
            }
        }
    }
}
