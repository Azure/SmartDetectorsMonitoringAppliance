//-----------------------------------------------------------------------
// <copyright file="SmartDetectorUnconfiguredProject.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace Microsoft.Azure.Monitoring.SmartDetectors.ProjectType
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.VisualStudio.ProjectSystem;
    using Microsoft.VisualStudio.ProjectSystem.VS;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Task = System.Threading.Tasks.Task;

    [Export]
    [AppliesTo(SmartDetectorUnconfiguredProject.UniqueCapability)]
    [ProjectTypeRegistration(VsPackage.ProjectTypeGuid, "SmartDetector", "#2", ProjectExtension, Language, resourcePackageGuid: VsPackage.PackageGuid, PossibleProjectExtensions = ProjectExtension, ProjectTemplatesDir = @"..\..\Templates\Projects\MyCustomProject")]
    [ProvideProjectItem(VsPackage.ProjectTypeGuid, "My Items", @"..\..\Templates\ProjectItems\MyCustomProject", 500)]
    internal class SmartDetectorUnconfiguredProject
    {
        /// <summary>
        /// The file extension used by your project type.
        /// This does not include the leading period.
        /// </summary>
        internal const string ProjectExtension = "csproj";

        /// <summary>
        /// A project capability that is present in your project type and none others.
        /// This is a convenient constant that may be used by your extensions so they
        /// only apply to instances of your project type.
        /// </summary>
        /// <remarks>
        /// This value should be kept in sync with the capability as actually defined in your .targets.
        /// </remarks>
        internal const string UniqueCapability = "SmartDetectors";

        internal const string Language = "SmartDetector";

        [ImportingConstructor]
        public SmartDetectorUnconfiguredProject(UnconfiguredProject unconfiguredProject)
        {
            this.ProjectHierarchies = new OrderPrecedenceImportCollection<IVsHierarchy>(projectCapabilityCheckProvider: unconfiguredProject);
        }

        [Import]
        internal UnconfiguredProject UnconfiguredProject { get; private set; }

        [Import]
        internal IActiveConfiguredProjectSubscriptionService SubscriptionService { get; private set; }

        [Import]
        internal IProjectThreadingService ProjectThreadingService { get; private set; }

        [Import]
        internal ActiveConfiguredProject<ConfiguredProject> ActiveConfiguredProject { get; private set; }

        [Import]
        internal ActiveConfiguredProject<SmartDetectorConfiguredProject> MyActiveConfiguredProject { get; private set; }

        [ImportMany(ExportContractNames.VsTypes.IVsProject, typeof(IVsProject))]
        internal OrderPrecedenceImportCollection<IVsHierarchy> ProjectHierarchies { get; private set; }

        internal IVsHierarchy ProjectHierarchy
        {
            get { return this.ProjectHierarchies.Single().Value; }
        }
    }
}
