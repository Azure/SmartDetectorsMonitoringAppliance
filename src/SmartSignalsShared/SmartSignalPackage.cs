//-----------------------------------------------------------------------
// <copyright file="SmartSignalPackage.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Smart Signal package, stored in the smart signals repository
    /// </summary>
    public sealed class SmartSignalPackage
    {
        private const string ManifestFileName = "Manifest.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalPackage"/> class.
        /// </summary>
        /// <param name="manifest">The signal's manifest</param>
        /// <param name="content">The signal package content represented by a dictionary mapping a file name to the file content bytes</param>
        public SmartSignalPackage(SmartSignalManifest manifest, IReadOnlyDictionary<string, byte[]> content)
        {
            this.Manifest = Diagnostics.EnsureArgumentNotNull(() => manifest);
            this.Content = Diagnostics.EnsureArgumentNotNull(() => content);
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
        /// <returns>A <see cref="SmartSignalPackage"/></returns>
        public static SmartSignalPackage CreateFromStream(Stream zippedPackageStream)
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
            SmartSignalManifest signalManifest = JsonConvert.DeserializeObject<SmartSignalManifest>(Encoding.UTF8.GetString(manifestBytes));

            return new SmartSignalPackage(signalManifest, packageContent);
        }
    }
}
