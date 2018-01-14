//-----------------------------------------------------------------------
// <copyright file="SmartSignalPackage.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Package
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Smart Signal package, stored in the smart signals repository
    /// </summary>
    public sealed class SmartSignalPackage
    {
        private const string ManifestFileName = "manifest.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalPackage"/> class.
        /// </summary>
        /// <param name="manifest">The signal's manifest</param>
        /// <param name="content">The signal package content represented by a dictionary mapping a file name to the file content bytes</param>
        public SmartSignalPackage(SmartSignalManifest manifest, IReadOnlyDictionary<string, byte[]> content)
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
        /// Gets the signal's package manifest.
        /// </summary>
        public SmartSignalManifest Manifest { get; }

        /// <summary>
        /// Gets the signal's package content represented by a dictionary mapping a file name to the file content bytes.
        /// </summary>
        public IReadOnlyDictionary<string, byte[]> Content { get; }
      
        /// <summary>
        /// Creates a <see cref="SmartSignalPackage"/> from a zipped package stream
        /// </summary>
        /// <param name="zippedPackageStream">The zipped package stream</param>
        /// <param name="tracer">The tracer</param>
        /// <returns>A <see cref="SmartSignalPackage"/></returns>
        public static SmartSignalPackage CreateFromStream(Stream zippedPackageStream, ITracer tracer)
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
                throw new InvalidSmartSignalPackageException("No manifest file found in the smart signal package");
            }

            // Deserialize the manifest
            string manifest = Encoding.UTF8.GetString(manifestBytes);
            tracer.TraceInformation($"Deserializing signal manifest {manifest}");
            SmartSignalManifest signalManifest = JsonConvert.DeserializeObject<SmartSignalManifest>(manifest);

            return new SmartSignalPackage(signalManifest, packageContent);
        }

        /// <summary>
        /// Creates a <see cref="SmartSignalPackage"/> from a folder
        /// </summary>
        /// <param name="sourceFolder">The folder of the package</param>
        /// <returns>A <see cref="SmartSignalPackage"/></returns>
        public static SmartSignalPackage CreateFromFolder(string sourceFolder)
        {
            string[] fileEntries = Directory.GetFiles(sourceFolder);
            var packageContent = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            
            // for each file in the package get the file name and content and add it to the result dictionary 
            foreach (string fileNamePath in fileEntries)
            {
                packageContent.Add(Path.GetFileName(fileNamePath), File.ReadAllBytes(fileNamePath));
            }
            
            bool manifestFileExists = packageContent.TryGetValue(ManifestFileName, out byte[] manifestBytes);
            if (!manifestFileExists)
            {
                throw new InvalidSmartSignalPackageException("Failed to create Smart Signal Package - no manifest file found in the smart signal package");
            }

            try
            {
                // Validates the manifest
                SmartSignalManifest signalManifest = JsonConvert.DeserializeObject<SmartSignalManifest>(Encoding.UTF8.GetString(manifestBytes));
                if (!packageContent.ContainsKey(signalManifest.AssemblyName))
                {
                    throw new InvalidSmartSignalPackageException("Failed to create Smart Signal Package - the manifest file is invalid: Assembly name must be a file in the smart signal package.");
                }

                if (!signalManifest.SupportedResourceTypes.Any())
                {
                    throw new InvalidSmartSignalPackageException("Failed to create Smart Signal Package - the manifest file is invalid: Must specify at least one supported resource type.");
                }

                if (!signalManifest.SupportedCadencesInMinutes.Any())
                {
                    throw new InvalidSmartSignalPackageException("Failed to create Smart Signal Package - the manifest file is invalid: Must specify at least one supported cadence.");
                }

                return new SmartSignalPackage(signalManifest, packageContent);
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidSmartSignalPackageException($"Failed to create Smart Signal Package - the manifest file is invalid: {argumentException.Message}");
            }
            catch (JsonException jsonException)
            {
                throw new InvalidSmartSignalPackageException($"Failed to create Smart Signal Package - the manifest file is invalid: {jsonException.Message}");
            }    
        }

        /// <summary>
        /// Saves a <see cref="SmartSignalPackage"/> object to a file.
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
