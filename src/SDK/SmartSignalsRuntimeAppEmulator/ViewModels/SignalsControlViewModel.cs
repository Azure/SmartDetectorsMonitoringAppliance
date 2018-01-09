//-----------------------------------------------------------------------
// <copyright file="SignalsControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="SignalsControl"/> control.
    /// </summary>
    public class SignalsControlViewModel : ObservableObject
    {
        private readonly AzureResourceManagerClient azureResourceManagerClient;

        private List<AzureSubscription> subscriptions;

        private AzureSubscription selectedSubscription;

        private List<string> resourceGroups;

        private string selectedResourceGroup;

        private List<string> resourceTypes;

        private string selectedResourceType;

        private List<string> resources;

        private string selectedResource;

        // Holds all user's azure resources identifiers according to selected subscription & resource group
        private List<ResourceIdentifier> resourcesIndentifiers;

        #region Ctros

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsControlViewModel"/> class for design time only.
        /// </summary>
        public SignalsControlViewModel()
        {
            this.Subscriptions = new List<AzureSubscription>() { new AzureSubscription("subscription_1_id", "subscription_1_displayName") };
            this.ResourceGroups = new List<string>() { "resourceGroups_1", "resourceGroups_2" };
            this.ResourcesTypes = new List<string>() { "resourceType_1", "resourceType_2" };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsControlViewModel"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services that were used to log in</param>
        [InjectionConstructor]
        public SignalsControlViewModel(AuthenticationServices authenticationServices)
        {
            AuthenticationResult authResult = authenticationServices.AuthenticationResult;
            var activeDirectoryCredentials = new ActiveDirectoryCredentials(authResult.AccessToken);
            this.azureResourceManagerClient = new AzureResourceManagerClient(activeDirectoryCredentials);
            this.GetSubscriptionsAsync();
        }

        #endregion

        #region Binded Properties
        /// <summary>
        /// Gets the user's subscriptions.
        /// </summary>
        public List<AzureSubscription> Subscriptions
        {
            get
            {
                return this.subscriptions;
            }

            private set
            {
                this.subscriptions = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the subscription selected by the user.
        /// </summary>
        public AzureSubscription SelectedSubscription
        {
            get
            {
                return this.selectedSubscription;
            }

            set
            {
                this.selectedSubscription = value;
                this.SelectedResourceGroup = null;
                this.SelectedResourceType = null;
                this.SelectedResource = null;
                this.OnPropertyChanged();
                this.GetResourceGroupsAsync();
            }
        }

        /// <summary>
        /// Gets the user's resources groups.
        /// </summary>
        public List<string> ResourceGroups
        {
            get
            {
                return this.resourceGroups;
            }

            private set
            {
                this.resourceGroups = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the resource group selected by the user.
        /// </summary>
        public string SelectedResourceGroup
        {
            get
            {
                return this.selectedResourceGroup;
            }

            set
            {
                this.selectedResourceGroup = value;
                this.OnPropertyChanged();
                this.SelectedResourceType = null;
                this.SelectedResource = null;

                if (value != null)
                {
                    this.GetResourcesTypesAsync();
                }
            }
        }

        /// <summary>
        /// Gets the user's resources types.
        /// </summary>
        public List<string> ResourcesTypes
        {
            get
            {
                return this.resourceTypes;
            }

            private set
            {
                this.resourceTypes = value;
                this.OnPropertyChanged();
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
                this.SelectedResource = null;

                if (value != null)
                {
                    try
                    {
                        ResourceType selectedResourceType = (ResourceType)Enum.Parse(typeof(ResourceType), this.SelectedResourceType);
                        this.Resources = this.resourcesIndentifiers.Where(resourceIndentifier => resourceIndentifier.ResourceType == selectedResourceType)
                                            .Select(resourceIndentifier => resourceIndentifier.ResourceName).ToList();
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    } 
                }
            }
        }

        /// <summary>
        /// Gets the user's resources.
        /// </summary>
        public List<string> Resources
        {
            get
            {
                return this.resources;
            }

            private set
            {
                this.resources = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the resource selected by the user.
        /// </summary>
        public string SelectedResource
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResource = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the subscriptions.
        /// </summary>
        private async void GetSubscriptionsAsync()
        {
            try
            {
                var subscriptions = (await this.azureResourceManagerClient.GetAllSubscriptionsAsync()).ToList();
                this.Subscriptions = subscriptions.Select(sub => new AzureSubscription(sub.Id, sub.DisplayName)).ToList();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        /// <summary>
        /// Gets or sets the resource groups.
        /// </summary>
        private async void GetResourceGroupsAsync()
        {
            try
            {
                var groupsIndentifiers = (await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(this.SelectedSubscription.Id, CancellationToken.None)).ToList();
                this.ResourceGroups = groupsIndentifiers.Select(ri => ri.ResourceGroupName).ToList();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        /// <summary>
        /// Gets or sets the resources types.
        /// </summary>
        private async void GetResourcesTypesAsync()
        {
            try
            {
                var supportedResourceTypes = new List<ResourceType>() { ResourceType.ApplicationInsights, ResourceType.LogAnalytics, ResourceType.VirtualMachine };
                this.resourcesIndentifiers = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(this.SelectedSubscription.Id, this.SelectedResourceGroup, supportedResourceTypes, CancellationToken.None)).ToList();
                var resourcesTypesGrouping = this.resourcesIndentifiers.GroupBy(resourceIndentifier => resourceIndentifier.ResourceType);
                this.ResourcesTypes = resourcesTypesGrouping.Select(group => group.Key.ToString()).ToList();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }
    }
}
