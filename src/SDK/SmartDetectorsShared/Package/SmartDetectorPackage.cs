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
        /// <param name="manifest">The Smart Detector's manifest</param>
        /// <param name="content">The Smart Detector package content represented by a dictionary mapping a file name to the file content bytes</param>
        public SmartDetectorPackage(SmartDetectorManifest manifest, IReadOnlyDictionary<string, byte[]> content)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            else if (content.Count == 0)
            {
                throw new ArgumentException("Package content must include at least one item", nameof(content));
            }

            this.Manifest = manifest;
            this.Content = content;
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
                    Stream entryStream = entry.Open();
                    using (var entryMemoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(entryMemoryStream);
                        byte[] entryBytes = entryMemoryStream.ToArray();
                        packageContent.Add(entry.FullName, entryBytes);
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
                SmartDetectorManifest smartDetectorManifest = JsonConvert.DeserializeObject<SmartDetectorManifest>(manifest);
                return new SmartDetectorPackage(smartDetectorManifest, packageContent);
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
                    if (!packageContent.ContainsKey(smartDetectorManifest.AssemblyName))
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

                    if (smartDetectorManifest.ImagePaths != null && smartDetectorManifest.ImagePaths.Any())
                    {
                        foreach (var imagePath in smartDetectorManifest.ImagePaths)
                        {
                            string path = imagePath.Replace("/", "\\");
                            if (!packageContent.ContainsKey(path))
                            {
                                throw new InvalidSmartDetectorPackageException($"Failed to create Smart Detector Package - The image file {imagePath} defined in the manifest file does not exists");
                            }
                        }
                    }

                    var assembly = Assembly.LoadFrom(Path.Combine(sourceFolder, smartDetectorManifest.AssemblyName));
                    if (assembly.GetType(smartDetectorManifest.ClassName) == null)
                    {
                        throw new InvalidSmartDetectorPackageException(
                            "Failed to create Smart Detector Package - the manifest file is invalid: The class name must be a file in the Smart Detector package.");
                    }

                    return new SmartDetectorPackage(smartDetectorManifest, packageContent);
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
    }
}
