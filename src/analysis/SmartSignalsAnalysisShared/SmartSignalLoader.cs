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
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalLoader"/> interface.
    /// </summary>
    public class SmartSignalLoader : ISmartSignalLoader
    {
        private readonly ISmartSignalsRepository smartSignalsRepository;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalLoader"/> class.
        /// </summary>
        /// <param name="smartSignalsRepository">The smart signals repository, used to read the signal metadata</param>
        /// <param name="tracer">The tracer</param>
        public SmartSignalLoader(ISmartSignalsRepository smartSignalsRepository, ITracer tracer)
        {
            this.smartSignalsRepository = Diagnostics.EnsureArgumentNotNull(() => smartSignalsRepository);
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
        }

        #region Implementation of ISmartSignalLoader

        /// <summary>
        /// Loads a Smart Signal. 
        /// This method load the signal's assembly into the current application domain,
        /// and creates the signal object using reflection.
        /// </summary>
        /// <param name="signalMetadata">The signal metadata.</param>
        /// <returns>The Smart Signal object.</returns>
        /// <exception cref="SmartSignalLoadException">
        /// Thrown if an error occurred during the signal load (either due to assembly load
        /// error or failure to create the signal object).
        /// </exception>
        public async Task<ISmartSignal> LoadSignalAsync(SmartSignalMetadata signalMetadata)
        {
            try
            {
                // Read the signal assemblies
                Dictionary<string, byte[]> signalAssemblies = await this.smartSignalsRepository.ReadSignalAssembliesAsync(signalMetadata.Id);
                this.tracer.TraceInformation($"Read {signalAssemblies.Count} assemblies for signal ID {signalMetadata.Id}");

                // Add assembly resolver, that uses the signal's assemblies
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    this.tracer.TraceInformation($"Resolving assembly {args.Name} for signal ID {signalMetadata.Id}");

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
                if (!signalAssemblies.TryGetValue(signalMetadata.AssemblyName, out byte[] signalMainAssemblyBytes))
                {
                    throw new SmartSignalLoadException($"Unable to find main signal assembly: {signalMetadata.AssemblyName}");
                }

                Assembly mainSignalAssembly = Assembly.Load(signalMainAssemblyBytes);

                // Get the signal type from the assembly
                this.tracer.TraceInformation($"Creating Smart signal for {signalMetadata.Name}, version {signalMetadata.Version}, using type {signalMetadata.ClassName}");
                Type signalType = mainSignalAssembly.GetType(signalMetadata.ClassName);
                if (signalType == null)
                {
                    throw new SmartSignalLoadException($"Signal type {signalMetadata.ClassName} was not found in the main signal assembly assembly {signalMetadata.AssemblyName}");
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
                        { "SignalId", signalMetadata.Id },
                        { "SignalName", signalMetadata.Name },
                        { "ExceptionType", e.GetType().Name },
                        { "ExceptionMessage", e.Message },
                    });

                throw new SmartSignalLoadException($"Failed to load smart signal {signalMetadata.Name}", e);
            }
        }

        #endregion
    }
}
