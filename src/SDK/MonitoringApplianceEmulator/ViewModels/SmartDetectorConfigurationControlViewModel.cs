//-----------------------------------------------------------------------
// <copyright file="SmartDetectorConfigurationControlViewModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Win32;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="SmartDetectorConfigurationControl"/> control.
    /// </summary>
    public class SmartDetectorConfigurationControlViewModel : ObservableObject
    {
        private readonly IExtendedAzureResourceManagerClient azureResourceManagerClient;

        private readonly SmartDetectorManifest smartDetectorManifest;

        private readonly ITracer tracer;

        private readonly NotificationService notificationService;

        private UserSettings userSettings;

        private IEmulationSmartDetectorRunner smartDetectorRunner;

        private string smartDetectorName;

        private ObservableCollection<SmartDetectorCadence> cadences;

        private SmartDetectorCadence selectedCadence;

        private ObservableTask<ObservableCollection<HierarchicalResource>> readSubscriptionsTask;

        private HierarchicalResource selectedSubscription;

        private ObservableCollection<string> supportedResourceTypes;

        private string selectedResourceType;

        private ObservableTask<List<ResourceIdentifier>> readResourcesTask;

        private HierarchicalResource resourcesHierarchicalCollection;

        private HierarchicalResource selectedResource;

        private bool shouldShowStatusControl;

        private bool iterativeRunModeEnabled;

        private DateTime iterativeStartTime;

        private DateTime iterativeEndTime;

        // Used when a dummy resource identifier is necessary for initialization purposes (should not be displayed)
        private ResourceIdentifier dummyResourceIdentifier;

        private bool shouldSelectResourcesAccordingToUserSettings;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControlViewModel"/> class for design time only.
        /// </summary>
        public SmartDetectorConfigurationControlViewModel()
        {
            this.ShouldShowStatusControl = true;
            this.SmartDetectorName = "SampleSmartDetector";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorConfigurationControlViewModel"/> class.
        /// </summary>
        /// <param name="azureResourceManagerClient">The Azure resources manager client.</param>
        /// <param name="tracer">The tracer.</param>
        /// <param name="smartDetectorManifest">The Smart Detector manifest.</param>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        /// <param name="userSettings">The user settings.</param>
        /// <param name="notificationService">The notification service.</param>
        [InjectionConstructor]
        public SmartDetectorConfigurationControlViewModel(
            IExtendedAzureResourceManagerClient azureResourceManagerClient,
            ITracer tracer,
            SmartDetectorManifest smartDetectorManifest,
            IEmulationSmartDetectorRunner smartDetectorRunner,
            UserSettings userSettings,
            NotificationService notificationService)
        {
            this.azureResourceManagerClient = azureResourceManagerClient;
            this.smartDetectorManifest = smartDetectorManifest;
            this.tracer = tracer;
            this.smartDetectorManifest = smartDetectorManifest;
            this.userSettings = userSettings;

            this.SmartDetectorRunner = smartDetectorRunner;
            this.SmartDetectorName = this.smartDetectorManifest.Name;
            this.ShouldShowStatusControl = false;
            this.notificationService = notificationService;

            // Create dummy resource identifier for initialization purposes
            this.dummyResourceIdentifier = new ResourceIdentifier(ResourceType.ApplicationInsights, "dummy-subscription-id", "dummy-resource-group-name", "dummy-resource-name");

            // Initialize cadences combo box
            IEnumerable<SmartDetectorCadence> cadences = this.smartDetectorManifest.SupportedCadencesInMinutes
                    .Select(cadence => new SmartDetectorCadence(TimeSpan.FromMinutes(cadence)));

            this.Cadences = new ObservableCollection<SmartDetectorCadence>(cadences);

            // Set selected cadence to be the first one. If non, pick 10 minutes cadence as default
            this.SelectedCadence = this.Cadences.Any() ?
                this.Cadences.First() :
                new SmartDetectorCadence(TimeSpan.FromMinutes(10));

            this.IterativeRunModeEnabled = false;
            this.IterativeStartTime = DateTime.UtcNow;
            this.IterativeEndTime = DateTime.UtcNow;

            // In case there is no subscription to select from user settings, skip the entire phase of re-loading resources from user settings
            if (this.userSettings.SelectedSubscription != null)
            {
                this.ShouldSelectResourcesAccordingToUserSettings = true;
            }

            this.SupportedResourceTypes = this.GetSupportedResourceTypes();

            this.ReadResourcesTask = new ObservableTask<List<ResourceIdentifier>>(
                Task.FromResult(new List<ResourceIdentifier>()),
                this.tracer);

            this.ReadSubscriptionsTask = new ObservableTask<ObservableCollection<HierarchicalResource>>(
                this.GetSubscriptionsAsync(),
                this.tracer,
                this.LoadPreviousSelectedSubscription);
        }

        #endregion

        #region Binded Properties

        /// <summary>
        /// Gets the Smart Detector name.
        /// </summary>
        public string SmartDetectorName
        {
            get
            {
                return this.smartDetectorName;
            }

            private set
            {
                this.smartDetectorName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Smart Detector is running or not.
        /// </summary>
        public bool ShouldShowStatusControl
        {
            get
            {
                return this.shouldShowStatusControl;
            }

            private set
            {
                this.shouldShowStatusControl = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public IEmulationSmartDetectorRunner SmartDetectorRunner
        {
            get
            {
                return this.smartDetectorRunner;
            }

            private set
            {
                this.smartDetectorRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a task that returns the user's subscriptions.
        /// </summary>
        public ObservableTask<ObservableCollection<HierarchicalResource>> ReadSubscriptionsTask
        {
            get
            {
                return this.readSubscriptionsTask;
            }

            private set
            {
                this.readSubscriptionsTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the subscription selected by the user.
        /// </summary>
        public HierarchicalResource SelectedSubscription
        {
            get
            {
                return this.selectedSubscription;
            }

            set
            {
                this.selectedSubscription = value;
                this.OnPropertyChanged();

                // Clean existing resources
                this.ResourcesHierarchicalCollection = null;
                this.SelectedResource = null;

                this.ReadResourcesTask = new ObservableTask<List<ResourceIdentifier>>(
                    this.GetResourcesInSubscriptionAsync(),
                    this.tracer);

                if (!this.ShouldSelectResourcesAccordingToUserSettings)
                {
                    this.userSettings.SelectedSubscription = value?.Name;
                }
            }
        }

        /// <summary>
        /// Gets a task that returns the Azure resources.
        /// </summary>
        public ObservableTask<List<ResourceIdentifier>> ReadResourcesTask
        {
            get
            {
                return this.readResourcesTask;
            }

            private set
            {
                this.readResourcesTask = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the resource types which supported by the detector.
        /// </summary>
        public ObservableCollection<string> SupportedResourceTypes
        {
            get
            {
                return this.supportedResourceTypes;
            }

            private set
            {
                this.supportedResourceTypes = value;
                this.OnPropertyChanged();

                this.SelectedResourceType = this.SupportedResourceTypes.First();

                if (this.ShouldSelectResourcesAccordingToUserSettings && this.userSettings.SelectedResourceType != null)
                {
                    string resourceTypeToSelect = this.supportedResourceTypes.SingleOrDefault(type => type == this.userSettings.SelectedResourceType);

                    if (resourceTypeToSelect != null)
                    {
                        this.SelectedResourceType = resourceTypeToSelect;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the resource type selected by the user.
        /// </summary>
        public string SelectedResourceType
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResourceType = value;
                this.OnPropertyChanged();

                if (!this.ShouldSelectResourcesAccordingToUserSettings)
                {
                    this.userSettings.SelectedResourceType = value;
                }

                if (this.ResourcesHierarchicalCollection != null && this.ResourcesHierarchicalCollection.ContainedResources != null)
                {
                    this.ResourcesHierarchicalCollection.ContainedResources.Filter = this.ShouldDisplayResource;
                }
            }
        }

        /// <summary>
        /// Gets or sets the resource selected by the user.
        /// </summary>
        public HierarchicalResource SelectedResource
        {
            get
            {
                return this.selectedResource;
            }

            set
            {
                this.selectedResource = value;
                this.OnPropertyChanged();

                if (!this.ShouldSelectResourcesAccordingToUserSettings)
                {
                    this.userSettings.SelectedResource = value?.Name;
                }
            }
        }

        /// <summary>
        /// Gets the resources hierarchical collection.
        /// </summary>
        public HierarchicalResource ResourcesHierarchicalCollection
        {
            get
            {
                return this.resourcesHierarchicalCollection;
            }

            private set
            {
                this.resourcesHierarchicalCollection = value;
                this.OnPropertyChanged();

                if (value != null)
                {
                    // Refresh the filter
                    this.resourcesHierarchicalCollection.ContainedResources.Filter = this.ShouldDisplayResource;

                    // Load selected resource from user settings if necessary
                    if (this.ShouldSelectResourcesAccordingToUserSettings && this.userSettings.SelectedResourceType != null && this.userSettings.SelectedResource != null)
                    {
                        string resourceTypeToSelect = this.supportedResourceTypes.SingleOrDefault(type => type == this.userSettings.SelectedResourceType);

                        if (resourceTypeToSelect != null)
                        {
                            HierarchicalResource resourceToSelect = this.resourcesHierarchicalCollection.TryFind(this.userSettings.SelectedResource);

                            if (resourceToSelect != null)
                            {
                                this.SelectedResource = resourceToSelect;
                            }
                        }
                    }

                    this.ShouldSelectResourcesAccordingToUserSettings = false;
                }
            }
        }

        /// <summary>
        /// Gets a task that returns the user's subscriptions.
        /// </summary>
        public ObservableCollection<SmartDetectorCadence> Cadences
        {
            get
            {
                return this.cadences;
            }

            private set
            {
                this.cadences = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the subscription selected by the user.
        /// </summary>
        public SmartDetectorCadence SelectedCadence
        {
            get
            {
                return this.selectedCadence;
            }

            set
            {
                this.selectedCadence = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether should execute iterative run mode.
        /// </summary>
        public bool IterativeRunModeEnabled
        {
            get
            {
                return this.iterativeRunModeEnabled;
            }

            set
            {
                this.iterativeRunModeEnabled = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the iterative start time.
        /// </summary>
        public DateTime IterativeStartTime
        {
            get
            {
                return this.iterativeStartTime;
            }

            set
            {
                this.iterativeStartTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the iterative end time.
        /// </summary>
        public DateTime IterativeEndTime
        {
            get
            {
                return this.iterativeEndTime;
            }

            set
            {
                this.iterativeEndTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resources should be auto-selected according to <see cref="UserSettings"/>.
        /// </summary>
        public bool ShouldSelectResourcesAccordingToUserSettings
        {
            get
            {
                return this.shouldSelectResourcesAccordingToUserSettings;
            }

            set
            {
                this.shouldSelectResourcesAccordingToUserSettings = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that runs the Smart Detector.
        /// </summary>
        public CommandHandler RunSmartDetectorCommand => new CommandHandler(this.RunSmartDetectorAsync);

        /// <summary>
        /// Gets the command that cancel the Smart Detector run.
        /// </summary>
        public CommandHandler CancelSmartDetectorRunCommand => new CommandHandler(() => this.SmartDetectorRunner.CancelSmartDetectorRun());

        /// <summary>
        /// Gets the command that handles a resource selection change.
        /// </summary>
        public CommandHandler OnSelectedResourceChangedCommand => new CommandHandler(this.SelectResource);

        /// <summary>
        /// Gets the command that imports the emulation run settings.
        /// </summary>
        public CommandHandler ImportCommand => new CommandHandler(this.Import);

        #endregion

        /// <summary>
        /// Handles a resource selection change.
        /// </summary>
        /// <param name="selectedResource">the selected resource</param>
        public void SelectResource(object selectedResource)
        {
            this.SelectedResource = (HierarchicalResource)selectedResource;
        }

        /// <summary>
        /// Load previous selected subscription from <see cref="UserSettings"/> and set selected subscription accordingly.
        /// In case there is no subscription to select, don't do anything.
        /// </summary>
        /// <param name="subscriptions">The subscriptions.</param>
        public void LoadPreviousSelectedSubscription(ObservableCollection<HierarchicalResource> subscriptions)
        {
            if (this.ShouldSelectResourcesAccordingToUserSettings &&
                this.userSettings.SelectedSubscription != null &&
                subscriptions != null)
            {
                HierarchicalResource subscriptionToSelect = subscriptions.SingleOrDefault(s => s.Name == this.userSettings.SelectedSubscription);

                if (subscriptionToSelect != null)
                {
                    this.SelectedSubscription = subscriptionToSelect;
                }
                else
                {
                    this.ShouldSelectResourcesAccordingToUserSettings = false;
                }
            }
            else if (this.userSettings.SelectedSubscription == null || subscriptions == null)
            {
                this.ShouldSelectResourcesAccordingToUserSettings = false;
            }
        }

        /// <summary>
        /// Indicates whether a <see cref="HierarchicalResource"/> should be displayed or not.
        /// </summary>
        /// <param name="resource">the resource</param>
        /// <returns>true if the resource should be displayed, false otherwise</returns>
        public bool ShouldDisplayResource(HierarchicalResource resource)
        {
            if (this.SelectedResourceType == "All")
            {
                return true;
            }

            ResourceType? selectedType = null;
            try
            {
                selectedType = (ResourceType)Enum.Parse(typeof(ResourceType), this.SelectedResourceType);
            }
            catch (Exception e)
            {
                Console.Write($"Failed to parse resource type {this.SelectedResourceType}: {e.Message}");
            }

            switch (resource.ResourceIdentifier.ResourceType)
            {
                case ResourceType.Subscription:
                    return true;
                case ResourceType.ResourceGroup:
                    if (selectedType == ResourceType.ResourceGroup)
                    {
                        return true;
                    }
                    else
                    {
                        return resource.ContainedResources.OriginalCollection.Any(r => r.ResourceIdentifier.ResourceType == selectedType);
                    }

                default:
                    return resource.ResourceIdentifier.ResourceType == selectedType;
            }
        }

        /// <summary>
        /// Raises a file selection dialog window to allow the user to select an emulation run settings file.
        /// </summary>
        /// <returns>The selected file path or null if no file was selected</returns>
        private static string GetEmulationRunSettingsFilePath()
        {
            // Get the folder for the roaming current user
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (Directory.Exists(Path.Combine(appDataFolderPath, "SmartAlertsEmulator")))
            {
                appDataFolderPath = Path.Combine(appDataFolderPath, "SmartAlertsEmulator");
            }

            var dialog = new OpenFileDialog()
            {
                InitialDirectory = appDataFolderPath
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// Gets Azure subscriptions.
        /// </summary>
        /// <returns>A task that returns the subscriptions</returns>
        private async Task<ObservableCollection<HierarchicalResource>> GetSubscriptionsAsync()
        {
            List<HierarchicalResource> subscriptions = (await this.azureResourceManagerClient.GetAllSubscriptionsAsync())
                .OrderBy(subscription => subscription.DisplayName)
                .Select(subscription =>
                {
                    var resourceIdentifier = new ResourceIdentifier(ResourceType.Subscription, subscription.Id, string.Empty, string.Empty);
                    return new HierarchicalResource(resourceIdentifier, new List<HierarchicalResource>(), subscription.DisplayName);
                }).ToList();

            return new ObservableCollection<HierarchicalResource>(subscriptions);
        }

        /// <summary>
        /// Gets the Azure resource types which supported by the detector.
        /// </summary>
        /// <returns>resource types</returns>
        private ObservableCollection<string> GetSupportedResourceTypes()
        {
            var supportedResourceTypes = new List<string>();

            if (this.smartDetectorManifest.SupportedResourceTypes.Count > 1)
            {
                supportedResourceTypes.Add("All");
            }

            supportedResourceTypes.AddRange(this.smartDetectorManifest.SupportedResourceTypes.Select(type => type.ToString()));

            return new ObservableCollection<string>(supportedResourceTypes);
        }

        /// <summary>
        /// Gets Azure resources.
        /// In addition to the returned task, the method is constructing the hierarchical collection and setting it in the <see cref="ResourcesHierarchicalCollection"/> property.
        /// </summary>
        /// <returns>A task that returns the resources</returns>
        private async Task<List<ResourceIdentifier>> GetResourcesInSubscriptionAsync()
        {
            List<ResourceType> supportedResourceTypes = this.smartDetectorManifest.SupportedResourceTypes
                .Where(type => type != ResourceType.Subscription && type != ResourceType.ResourceGroup).ToList();

            // Get supported resources
            List<ResourceIdentifier> resources = new List<ResourceIdentifier>();
            if (supportedResourceTypes.Count > 0)
            {
                resources = (await this.azureResourceManagerClient.GetAllResourcesInSubscriptionAsync(
                    this.SelectedSubscription.ResourceIdentifier.SubscriptionId,
                    supportedResourceTypes,
                    CancellationToken.None)).ToList();
            }

            // Add resource groups
            resources.AddRange(await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(
                    this.SelectedSubscription.ResourceIdentifier.SubscriptionId,
                    CancellationToken.None));

            // Model resources in an hierarchical collection
            var resourcesByGroups = resources.GroupBy(resource => resource.ResourceGroupName)
                .Select(group =>
                {
                    // Create a list of all resources in group (filter out the group itself)
                    var allResourcesInGroup = group
                        .Where(resource => resource.ResourceType != ResourceType.ResourceGroup)
                        .Select(resource => new HierarchicalResource(resource, new List<HierarchicalResource>(), resource.ResourceName));

                    // Find the group's resource identifier - since every resource (also group) should hold its own resource identifier
                    // In some cases there is no resource identifier for the group - in such cases create a new one to keep hierarchical structure.
                    ResourceIdentifier groupResourceIdentifier;
                    bool doesGroupIdentifierExist = group.ToList().Any(resource =>
                        resource.ResourceType == ResourceType.ResourceGroup &&
                        resource.ResourceGroupName == group.Key);

                    if (doesGroupIdentifierExist)
                    {
                        groupResourceIdentifier = group.ToList().Find(resource =>
                            resource.ResourceType == ResourceType.ResourceGroup &&
                            resource.ResourceGroupName == group.Key);
                    }
                    else
                    {
                        groupResourceIdentifier = new ResourceIdentifier(
                            resourceType: ResourceType.ResourceGroup,
                            subscriptionId: this.selectedSubscription.ResourceIdentifier.SubscriptionId,
                            resourceGroupName: group.Key,
                            resourceName: string.Empty);
                    }

                    // Add the resource group
                    return new HierarchicalResource(groupResourceIdentifier, allResourcesInGroup.ToList(), groupResourceIdentifier.ResourceGroupName);
                });

            // Create a resource for the root subscription
            var subscriptionResource = new HierarchicalResource(this.SelectedSubscription.ResourceIdentifier, resourcesByGroups.ToList(), this.SelectedSubscription.Name);

            // Create dummy root resource that contains the root subscription
            var dummyRoot = new HierarchicalResource(this.dummyResourceIdentifier, new List<HierarchicalResource>() { subscriptionResource }, "dummy-name");

            this.ResourcesHierarchicalCollection = dummyRoot;

            // return all resources
            return resources;
        }

        /// <summary>
        /// Runs the Smart Detector.
        /// </summary>
        private async void RunSmartDetectorAsync()
        {
            this.ShouldShowStatusControl = true;

            DateTime startTimeRange, endTimeRange;
            if (this.IterativeRunModeEnabled)
            {
                startTimeRange = this.IterativeStartTime;
                endTimeRange = this.IterativeEndTime;
            }
            else
            {
                // If we're running in iterative mode, use the current time for running the detector
                startTimeRange = endTimeRange = DateTime.UtcNow;
            }

            try
            {
                await this.smartDetectorRunner.RunAsync(
                    this.selectedResource,
                    this.ReadResourcesTask.Result,
                    this.SelectedCadence.TimeSpan,
                    startTimeRange,
                    endTimeRange,
                    this.userSettings,
                    this.SelectedSubscription.ResourceIdentifier.SubscriptionId);
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Failed running Detector: {e.Message}");
            }
        }

        /// <summary>
        /// Imports the emulation run settings.
        /// </summary>
        private void Import()
        {
            // Get the emulation run settings file path
            string filePath = GetEmulationRunSettingsFilePath();

            if (filePath != null)
            {
                // Load the file and create an emulation run settings object from the JSON file
                EmulationRunSettings emulationRunSettings = EmulationRunSettings.LoadEmulationRunSettings(filePath);

                if (emulationRunSettings != null)
                {
                    this.userSettings = emulationRunSettings.UserSettings;
                    this.ShouldSelectResourcesAccordingToUserSettings = true;

                    // Load the selected subscription
                    this.ReadSubscriptionsTask = new ObservableTask<ObservableCollection<HierarchicalResource>>(
                        this.GetSubscriptionsAsync(),
                        this.tracer,
                        this.LoadPreviousSelectedSubscription);

                    // Set the iterative run mode, cadence, start time, and end time
                    if (emulationRunSettings.IterativeRunModeEnabled)
                    {
                        this.IterativeRunModeEnabled = true;
                        this.SelectedCadence = this.Cadences.First(cad => cad.DisplayName == emulationRunSettings.AnalysisCadence.DisplayName);
                        this.IterativeStartTime = emulationRunSettings.StartTime;
                        this.IterativeEndTime = emulationRunSettings.EndTime;
                    }
                    else
                    {
                        this.IterativeRunModeEnabled = false;

                        // Set selected cadence to be the first one. If non, pick 10 minutes cadence as default
                        this.SelectedCadence = this.Cadences.Any() ?
                            this.Cadences.First() :
                            new SmartDetectorCadence(TimeSpan.FromMinutes(10));

                        this.IterativeStartTime = DateTime.UtcNow;
                        this.IterativeEndTime = DateTime.UtcNow;
                    }

                    // Switch to the alerts view
                    this.notificationService.OnTabSwitchedToAlertsControl();

                    // Load the alerts from the emulation run settings
                    this.SmartDetectorRunner.Alerts.Clear();
                    foreach (var alert in emulationRunSettings.EmulationAlerts)
                    {
                        this.SmartDetectorRunner.Alerts.Add(alert);
                    }

                    // Set the emulation run settings in the smart detector runner
                    this.SmartDetectorRunner.EmulationRunSettings = emulationRunSettings;
                }
            }
        }
    }
}
