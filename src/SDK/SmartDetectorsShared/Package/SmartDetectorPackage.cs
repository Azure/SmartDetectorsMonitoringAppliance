//-----------------------------------------------------------------------
// <copyright file="SmartDetectorPackage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Package
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Smart Detector package, stored in the Smart Detectors repository
    /// </summary>
    public sealed class SmartDetectorPackage
    {
        private const string ManifestFileName = "manifest.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorPackage"/> class.
        /// </summary>
        /// <param name="smartDetectorManifest">The Smart Detector's manifest</param>
        /// <param name="packageContent">The Smart Detector package content represented by a dictionary mapping a file name to the file content bytes</param>
        public SmartDetectorPackage(SmartDetectorManifest smartDetectorManifest, IReadOnlyDictionary<string, byte[]> packageContent)
        {
            ValidatePackage(smartDetectorManifest, packageContent);

            this.Manifest = smartDetectorManifest;
            this.Content = packageContent;
        }

        /// <summary>
        /// Gets the Smart Detector's package manifest.
        /// </summary>
        public SmartDetectorManifest Manifest { get; }

        /// <summary>
        /// Gets the Smart Detector's package content represented by a dictionary mapping a file name to the file content bytes.
        /// </summary>
        public IReadOnlyDictionary<string, byte[]> Content { get; }

        /// <summary>
        /// Creates a <see cref="SmartDetectorPackage"/> from a zipped package stream
        /// </summary>
        /// <param name="zippedPackageStream">The zipped package stream</param>
        /// <param name="tracer">The tracer</param>
        /// <returns>A <see cref="SmartDetectorPackage"/></returns>
        public static SmartDetectorPackage CreateFromStream(Stream zippedPackageStream, ITracer tracer)
        {
            var packageContent = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (var archive = new ZipArchive(zippedPackageStream, ZipArchiveMode.Read))
            {
                // for each file in the package get the file name and content and add it to the result dictionary
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    using (Stream entryStream = entry.Open())
                    {
                        using (var entryMemoryStream = new MemoryStream())
                        {
                            entryStream.CopyTo(entryMemoryStream);
                            byte[] entryBytes = entryMemoryStream.ToArray();
                            packageContent.Add(entry.FullName, entryBytes);
                        }
                    }
                }
            }

            bool manifestFileExists = packageContent.TryGetValue(ManifestFileName, out byte[] manifestBytes);
            if (!manifestFileExists)
            {
                throw new InvalidSmartDetectorPackageException("No manifest file found in the Smart Detector package");
            }

            // Deserialize the manifest. We use stream reader to avoid unexpected characters such as BOM.
            using (var stream = new StreamReader(new MemoryStream(manifestBytes)))
            {
                string manifest = stream.ReadToEnd();
                tracer.TraceInformation($"Deserializing Smart Detector manifest {manifest}");
                try
                {
                    SmartDetectorManifest smartDetectorManifest = JsonConvert.DeserializeObject<SmartDetectorManifest>(manifest);
                    return new SmartDetectorPackage(smartDetectorManifest, packageContent);
                }
                catch (JsonException jsonException)
                {
                    throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - the manifest file is invalid: {jsonException.Message}");
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="SmartDetectorPackage"/> from a folder
        /// </summary>
        /// <param name="sourceFolder">The folder of the package</param>
        /// <returns>A <see cref="SmartDetectorPackage"/></returns>
        public static SmartDetectorPackage CreateFromFolder(string sourceFolder)
        {
            string[] fileEntries = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
            var packageContent = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            // for each file in the package get the file relative path and content and add it to the result dictionary
            foreach (string filePath in fileEntries)
            {
                string relativePath = filePath.Substring(sourceFolder.Length).TrimStart('\\');
                if (Path.GetExtension(relativePath) != ".package")
                {
                    packageContent.Add(relativePath, File.ReadAllBytes(filePath));
                }
            }

            bool manifestFileExists = packageContent.TryGetValue(ManifestFileName, out byte[] manifestBytes);
            if (!manifestFileExists)
            {
                throw new InvalidSmartDetectorPackageException("Failed to create Smart Detector Package - no manifest file found in the Smart Detector package");
            }

            try
            {
                // Validates the manifest. We use stream reader to avoid unexpected characters such as BOM.
                using (var stream = new StreamReader(new MemoryStream(manifestBytes)))
                {
                    string manifest = stream.ReadToEnd();
                    SmartDetectorManifest smartDetectorManifest = JsonConvert.DeserializeObject<SmartDetectorManifest>(manifest);

                    var package = new SmartDetectorPackage(smartDetectorManifest, packageContent);

                    // After the package is created, we validate that the detector's class exists there
                    var assembly = Assembly.LoadFrom(Path.Combine(sourceFolder, package.Manifest.AssemblyName));
                    if (assembly.GetType(package.Manifest.ClassName) == null)
                    {
                        throw new InvalidSmartDetectorPackageException(
                            "Failed to create Smart Detector Package - the manifest file is invalid: The class name must be a file in the Smart Detector package.");
                    }

                    return package;
                }
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - the manifest file is invalid: {argumentException.Message}");
            }
            catch (JsonException jsonException)
            {
                throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - the manifest file is invalid: {jsonException.Message}");
            }
        }

        /// <summary>
        /// Saves a <see cref="SmartDetectorPackage"/> object to a file.
        /// </summary>
        /// <param name="targetPath">The path to save the package to.</param>
        public void SaveToFile(string targetPath)
        {
            using (var fileStream = new FileStream(targetPath, FileMode.Create))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (KeyValuePair<string, byte[]> attachment in this.Content)
                    {
                        var zipEntry = zipArchive.CreateEntry(attachment.Key);
                        using (var entryStream = zipEntry.Open())
                        using (var streamWriter = new BinaryWriter(entryStream))
                        {
                            streamWriter.Write(attachment.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks that the content of the package is valid
        /// </summary>
        /// <param name="smartDetectorManifest">The Smart Detector's manifest</param>
        /// <param name="packageContent">The Smart Detector package content represented by a dictionary mapping a file name to the file content bytes</param>
        private static void ValidatePackage(SmartDetectorManifest smartDetectorManifest, IReadOnlyDictionary<string, byte[]> packageContent)
        {
            if (smartDetectorManifest == null)
            {
                throw new ArgumentNullException(nameof(smartDetectorManifest));
            }

            if (packageContent == null)
            {
                throw new ArgumentNullException(nameof(packageContent));
            }
            else if (packageContent.Count == 0)
            {
                throw new ArgumentException("Package content must include at least one item", nameof(packageContent));
            }

            if (!(packageContent.ContainsKey(smartDetectorManifest.AssemblyName) ||
                  packageContent.ContainsKey(smartDetectorManifest.AssemblyName + ".dll") ||
                  packageContent.ContainsKey(smartDetectorManifest.AssemblyName + ".exe")))
            {
                throw new InvalidSmartDetectorPackageException("Failed to create Smart Detector Package - the manifest file is invalid: Assembly name must be a file in the Smart Detector package.");
            }

            if (smartDetectorManifest.SupportedResourceTypes == null || !smartDetectorManifest.SupportedResourceTypes.Any())
            {
                throw new InvalidSmartDetectorPackageException("Failed to create Smart Detector Package - the manifest file is invalid: Must specify at least one supported resource type.");
            }

            if (smartDetectorManifest.SupportedCadencesInMinutes == null || !smartDetectorManifest.SupportedCadencesInMinutes.Any())
            {
                throw new InvalidSmartDetectorPackageException("Failed to create Smart Detector Package - the manifest file is invalid: Must specify at least one supported cadence.");
            }

            if (smartDetectorManifest.ImagePaths != null)
            {
                foreach (var imagePath in smartDetectorManifest.ImagePaths)
                {
                    string path = imagePath.Replace("/", "\\");
                    if (!packageContent.ContainsKey(path))
                    {
                        throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - the image file {imagePath} defined in the manifest file does not exists");
                    }
                }
            }

            if (smartDetectorManifest.ParametersDefinitions != null)
            {
                foreach (DetectorParameterDefinition parameterDefinition in smartDetectorManifest.ParametersDefinitions)
                {
                    if (string.IsNullOrEmpty(parameterDefinition.Name))
                    {
                        throw new InvalidSmartDetectorPackageException("Failed to create Smart Detector Package - got parameter definition with no name");
                    }

                    if (string.IsNullOrEmpty(parameterDefinition.DisplayName))
                    {
                        throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - parameter definition '{parameterDefinition.Name}' has no display name");
                    }
                }
            }
        }
    }
}
