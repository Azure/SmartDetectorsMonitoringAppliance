//-----------------------------------------------------------------------
// <copyright file="SmartSignalLoader.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalLoader"/> interface.
    /// </summary>
    public class SmartSignalLoader : ISmartSignalLoader
    {
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalLoader"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        public SmartSignalLoader(ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
        }

        #region Implementation of ISmartSignalLoader

        /// <summary>
        /// Loads a Smart Signal. 
        /// This method load the signal's assembly into the current application domain,
        /// and creates the signal object using reflection.
        /// </summary>
        /// <param name="signalPackage">The signal package.</param>
        /// <returns>The Smart Signal object.</returns>
        /// <exception cref="SmartSignalLoadException">
        /// Thrown if an error occurred during the signal load (either due to assembly load
        /// error or failure to create the signal object).
        /// </exception>
        public ISmartSignal LoadSignal(SmartSignalPackage signalPackage)
        {
            SmartSignalManifest signalManifest = signalPackage.Manifest;
            IReadOnlyDictionary<string, byte[]> signalAssemblies = signalPackage.Content;
            try
            {
                this.tracer.TraceInformation($"Read {signalAssemblies.Count} assemblies for signal ID {signalManifest.Id}");

                // Add assembly resolver, that uses the signal's assemblies
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    this.tracer.TraceInformation($"Resolving assembly {args.Name} for signal ID {signalManifest.Id}");

                    // Get the short name of the assembly (AssemblyName.Name)
                    AssemblyName assemblyName = new AssemblyName(args.Name);
                    string name = assemblyName.Name;

                    // Try to find the assembly bytes in the signal's assemblies
                    if (signalAssemblies.TryGetValue(name, out byte[] assemblyBytes))
                    {
                        // Load the assembly from its bytes
                        return Assembly.Load(assemblyBytes);
                    }

                    return null;
                };

                // Find the main signal assembly
                if (!signalAssemblies.TryGetValue(signalManifest.AssemblyName, out byte[] signalMainAssemblyBytes))
                {
                    throw new SmartSignalLoadException($"Unable to find main signal assembly: {signalManifest.AssemblyName}");
                }

                Assembly mainSignalAssembly = Assembly.Load(signalMainAssemblyBytes);

                // Get the signal type from the assembly
                this.tracer.TraceInformation($"Creating Smart signal for {signalManifest.Name}, version {signalManifest.Version}, using type {signalManifest.ClassName}");
                Type signalType = mainSignalAssembly.GetType(signalManifest.ClassName);
                if (signalType == null)
                {
                    throw new SmartSignalLoadException($"Signal type {signalManifest.ClassName} was not found in the main signal assembly {signalManifest.AssemblyName}");
                }

                // Check if the type inherits from ISmartSignal
                if (!typeof(ISmartSignal).IsAssignableFrom(signalType))
                {
                    throw new SmartSignalLoadException($"Signal type {signalType.Name} does not extend ISmartSignal");
                }

                // Check that type is not abstract
                if (signalType.IsAbstract)
                {
                    throw new SmartSignalLoadException($"Signal type {signalType.Name} is abstract - a smart signal must be a concrete type");
                }

                // Check that type is not generic
                if (signalType.IsGenericTypeDefinition)
                {
                    throw new SmartSignalLoadException($"Signal type {signalType.Name} is generic - a smart signal must be a closed constructed type");
                }

                // Check that type has a parameter-less constructor
                if (signalType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new SmartSignalLoadException($"Signal type {signalType.Name} does not have a public, parameter-less constructor");
                }

                // Create the signal object
                ISmartSignal signal = Activator.CreateInstance(signalType) as ISmartSignal;
                if (signal == null)
                {
                    throw new SmartSignalLoadException($"Signal {signalType.Name} failed to be created - instance is null");
                }

                this.tracer.TraceInformation($"Successfully created signal of type {signalType.Name}");
                return signal;
            }
            catch (Exception e)
            {
                this.tracer.TrackEvent(
                    "FailedToLoadSignal",
                    properties: new Dictionary<string, string>
                    {
                        { "SignalId", signalManifest.Id },
                        { "SignalName", signalManifest.Name },
                        { "ExceptionType", e.GetType().Name },
                        { "ExceptionMessage", e.Message },
                    });

                throw new SmartSignalLoadException($"Failed to load smart signal {signalManifest.Name}", e);
            }
        }

        #endregion
    }
}
