//-----------------------------------------------------------------------
// <copyright file="SmartDetectorLoader.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Loader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// Implementation of the <see cref="ISmartDetectorLoader"/> interface. This class is
    /// used to load a smart detector package and create an instance of the detector.
    /// </summary>
    public class SmartDetectorLoader : ISmartDetectorLoader
    {
        private readonly string tempFolder;
        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoader"/> class.
        /// </summary>
        /// <param name="tempFolder">The temp folder to use for storing assembly files</param>
        /// <param name="tracer">The tracer</param>
        public SmartDetectorLoader(string tempFolder, IExtendedTracer tracer)
        {
            this.tempFolder = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => tempFolder);
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
        }

        #region Implementation of ISmartDetectorLoader

        /// <summary>
        /// Loads a Smart Detector.
        /// This method load the Smart Detector's assembly into the current application domain,
        /// and creates the Smart Detector object using reflection.
        /// </summary>
        /// <param name="smartDetectorPackage">The Smart Detector package.</param>
        /// <returns>The Smart Detector object.</returns>
        /// <exception cref="SmartDetectorLoadException">
        /// Thrown if an error occurred during the Smart Detector load (either due to assembly load
        /// error or failure to create the Smart Detector object).
        /// </exception>
        public ISmartDetector LoadSmartDetector(SmartDetectorPackage smartDetectorPackage)
        {
            SmartDetectorManifest smartDetectorManifest = smartDetectorPackage.Manifest;

            try
            {
                this.tracer.TraceInformation($"Read {smartDetectorPackage.Content.Count} assemblies for Smart Detector ID {smartDetectorManifest.Id}");

                // Write all DLLs to the temp folder
                foreach (var assemblyNameAndBytes in smartDetectorPackage.Content)
                {
                    string fileName = Path.Combine(this.tempFolder, assemblyNameAndBytes.Key);
                    Directory.CreateDirectory(Directory.GetParent(fileName).FullName);
                    File.WriteAllBytes(fileName, assemblyNameAndBytes.Value);
                }

                // Add assembly resolver, that uses the Smart Detector's assemblies
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    this.tracer.TraceInformation($"Resolving assembly {args.Name} for Smart Detector ID {smartDetectorManifest.Id}");
                    return this.LoadAssembly(args.Name);
                };

                // Load the main Smart Detector assembly
                Assembly mainSmartDetectorAssembly = this.LoadAssembly(smartDetectorManifest.AssemblyName);
                if (mainSmartDetectorAssembly == null)
                {
                    throw new SmartDetectorLoadException($"Unable to find main Smart Detector assembly: {smartDetectorManifest.AssemblyName}");
                }

                // Get the Smart Detector type from the assembly
                this.tracer.TraceInformation($"Creating Smart Detector for {smartDetectorManifest.Name}, version {smartDetectorManifest.Version}, using type {smartDetectorManifest.ClassName}");
                Type smartDetectorType = mainSmartDetectorAssembly.GetType(smartDetectorManifest.ClassName);
                if (smartDetectorType == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorManifest.ClassName} was not found in the main Smart Detector assembly {smartDetectorManifest.AssemblyName}");
                }

                // Check if the type inherits from ISmartDetector
                if (!typeof(ISmartDetector).IsAssignableFrom(smartDetectorType))
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} does not extend ISmartDetector");
                }

                // Check that type is not abstract
                if (smartDetectorType.IsAbstract)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} is abstract - a Smart Detector must be a concrete type");
                }

                // Check that type is not generic
                if (smartDetectorType.IsGenericTypeDefinition)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} is generic - a Smart Detector must be a closed constructed type");
                }

                // Check that type has a parameter-less constructor
                if (smartDetectorType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} does not have a public, parameter-less constructor");
                }

                // Create the Smart Detector object
                ISmartDetector smartDetector = Activator.CreateInstance(smartDetectorType) as ISmartDetector;
                if (smartDetector == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector {smartDetectorType.Name} failed to be created - instance is null");
                }

                this.tracer.TraceInformation($"Successfully created Smart Detector of type {smartDetectorType.Name}");
                return smartDetector;
            }
            catch (Exception e)
            {
                this.tracer.TrackEvent(
                    "FailedToLoadSmartDetector",
                    properties: new Dictionary<string, string>
                    {
                        { "smartDetectorId", smartDetectorManifest.Id },
                        { "SmartDetectorName", smartDetectorManifest.Name },
                        { "ExceptionType", e.GetType().Name },
                        { "ExceptionMessage", e.Message },
                    });

                throw new SmartDetectorLoadException($"Failed to load Smart Detector {smartDetectorManifest.Name}", e);
            }
        }

        /// <summary>
        /// Load an assembly from the temporary folder
        /// </summary>
        /// <param name="assemblyNameString">The assembly name</param>
        /// <returns>The loaded assembly</returns>
        private Assembly LoadAssembly(string assemblyNameString)
        {
            // Get the short name of the assembly (AssemblyName.Name)
            AssemblyName assemblyName = new AssemblyName(assemblyNameString);
            string name = assemblyName.Name;

            // Search for the file in the temp folder
            string fileName = Path.Combine(this.tempFolder, name);
            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }

            fileName = Path.Combine(this.tempFolder, name + ".dll");
            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }

            fileName = Path.Combine(this.tempFolder, name + ".exe");
            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }

            return null;
        }

        #endregion
    }
}