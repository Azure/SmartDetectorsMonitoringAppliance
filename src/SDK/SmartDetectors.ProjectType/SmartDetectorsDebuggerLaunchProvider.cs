//-----------------------------------------------------------------------
// <copyright file="SmartDetectorsDebuggerLaunchProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    [ExportDebugger(SmartDetectorsDebugger.SchemaName)]
    [AppliesTo(SmartDetectorUnconfiguredProject.UniqueCapability)]
    public class SmartDetectorsDebuggerLaunchProvider : DebugLaunchProviderBase
    {
        [ImportingConstructor]
        public SmartDetectorsDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("SmartDetector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9be6e469bc4921f1", "XamlRuleToCode:SmartDetectorsDebugger.xaml", "Project")]
        [AppliesTo(SmartDetectorUnconfiguredProject.UniqueCapability)]
        private object DebuggerXaml { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Gets project properties that the debugger needs to launch.
        /// </summary>
        [Import]
        private ProjectProperties DebuggerProperties { get; set; }

        public override async Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            var properties = await this.DebuggerProperties.GetSmartDetectorsDebuggerPropertiesAsync();
            string commandValue = await properties.SmartDetectorsDebuggerCommand.GetEvaluatedValueAtEndAsync();
            return !string.IsNullOrEmpty(commandValue);
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);

            // The properties that are available via DebuggerProperties are determined by the property XAML files in your project.
            var debuggerProperties = await this.DebuggerProperties.GetSmartDetectorsDebuggerPropertiesAsync();
            settings.Executable = await debuggerProperties.SmartDetectorsDebuggerCommand.GetEvaluatedValueAtEndAsync();
            settings.Arguments = await debuggerProperties.SmartDetectorsDebuggerCommandArguments.GetEvaluatedValueAtEndAsync();
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.LaunchDebugEngineGuid = DebuggerEngines.ManagedOnlyEngine;

            return new IDebugLaunchSettings[] { settings };
        }
    }
}
