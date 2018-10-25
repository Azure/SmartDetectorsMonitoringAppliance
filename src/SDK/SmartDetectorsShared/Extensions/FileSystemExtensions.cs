//-----------------------------------------------------------------------
// <copyright file="FileSystemExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;

    /// <summary>
    /// Extension methods for file system objects
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Create a temporary folder, under the current user's temp path and the specified sub folder
        /// </summary>
        /// <param name="subFolderName">The sub folder name</param>
        /// <returns>The temp folder</returns>
        public static string CreateTempFolder(string subFolderName)
        {
            string parentFolder = GetUserTempFolder(subFolderName);
            string tempFolder = Path.Combine(parentFolder, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            return tempFolder;
        }

        /// <summary>
        /// Cleanup the temp folders - delete all folders under the temp path and the specified sub folder, that were created before the specified duration.
        /// </summary>
        /// <param name="subFolderName">The sub folder name</param>
        /// <param name="maximalFolderAgeInHours">The maximal age of a folder, in hours - all folders that are older than this will be deleted</param>
        /// <param name="tracer">The tracer to use</param>
        public static void CleanupTempFolders(string subFolderName, int maximalFolderAgeInHours = 6, IExtendedTracer tracer = null)
        {
            // Delete the all folders under the temp path and sub folder that were created over 6 hours ago
            // This handles scenarios of a previous crash that prevented the folder from being deleted
            // All deletion errors are ignored - if the folder is in use, it should not be deleted
            DirectoryInfo parentFolder = new DirectoryInfo(GetUserTempFolder(subFolderName));
            parentFolder.GetDirectories()
                .Where(subFolder => (DateTime.UtcNow - subFolder.CreationTimeUtc).TotalHours > maximalFolderAgeInHours)
                .ToList()
                .ForEach(subFolder => TryDeleteFolder(subFolder.FullName, tracer));
        }

        /// <summary>
        /// Try to delete the specified folder - does not throw exception in case of failure, only traces
        /// </summary>
        /// <param name="folder">The folder to delete</param>
        /// <param name="tracer">The tracer to use</param>
        /// <returns>True if the folder was successfully deleted, false otherwise</returns>
        public static bool TryDeleteFolder(string folder, IExtendedTracer tracer = null)
        {
            try
            {
                Directory.Delete(folder, true);
                tracer?.TraceInformation($"Successfully deleted folder {folder}");
                return true;
            }
            catch (Exception e)
            {
                tracer?.TraceWarning($"Could not delete folder {folder}, error: {e.Message}");
                tracer?.ReportException(e);
                return false;
            }
        }

        /// <summary>
        /// Gets the path of the specified sub folder in the current user's temp folder
        /// </summary>
        /// <param name="subFolderName">The sub folder name</param>
        /// <returns>The user's temp folder path</returns>
        private static string GetUserTempFolder(string subFolderName)
        {
            string parentFolder = Path.GetTempPath();

            if (subFolderName != null)
            {
                parentFolder = Path.Combine(parentFolder, subFolderName);
            }

            return parentFolder;
        }
    }
}