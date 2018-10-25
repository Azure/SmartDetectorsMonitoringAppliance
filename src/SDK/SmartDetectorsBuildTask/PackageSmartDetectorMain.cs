//-----------------------------------------------------------------------
// <copyright file="PackageSmartDetectorMain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Build
{
    using System;
    using System.IO;
    using System.Security;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// The main class of the Smart Detector build process.
    /// </summary>
    public class PackageSmartDetectorMain
    {
        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">Command line arguments. These arguments are expected to be created by <see cref="PackageSmartDetector.Execute"/>.</param>
        /// <returns>The exit code; 0 if the task successfully executed; otherwise, 1</returns>
        private static int Main(string[] args)
        {
            try
            {
                var packagePath = args[0];
                var packageName = args[1];
                SmartDetectorPackage package = SmartDetectorPackage.CreateFromFolder(packagePath);
                package.SaveToFile(Path.Combine(packagePath, packageName));
                return 0;
            }
            catch (InvalidSmartDetectorPackageException exception)
            {
                Console.Write(exception.Message);
                return 1;
            }
            catch (IOException ioe)
            {
                Console.Write($"Failed to create Smart Detector Package - failed creating the package file: {ioe.Message}");
                return 1;
            }
            catch (SecurityException securityException)
            {
                Console.Write($"Failed to create Smart Detector Package - failed creating the package file: {securityException.Message}");
                return 1;
            }
            catch (Exception exception)
            {
                Console.Write(exception.Message);
                return 1;
            }
        }
    }
}
