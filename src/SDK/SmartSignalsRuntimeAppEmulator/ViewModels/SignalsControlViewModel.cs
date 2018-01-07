

using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;

    public class SignalsControlViewModel : ObservableObject
    {
        private List<string> subscriptions;

        private string selectedSubscription;

        private List<string> resourceGroups;

        private string selectedResourceGroup;

        private List<string> resourceTypes;

        private string selectedResourceType;

        private List<string> resources;

        private string selectedResource;

        private List<ResourceIdentifier> resourcesIndentifiers;

        private AuthenticationServices authenticationServices;

        private AzureResourceManagerClient azureResourceManagerClient;

        public SignalsControlViewModel()
        {
            this.Subscriptions = new List<string>() { "e0b6713a-be99-421c-ab05-8277c6d8b02d", "subscription_2" };
            this.ResourceGroups = new List<string>() { "resourceGroups_1", "resourceGroups_2" };
            this.ResourcesTypes = new List<string>() { "resourceType_1", "resourceType_2" };

        }

        public SignalsControlViewModel(AuthenticationServices authenticationServices)
        {
            this.authenticationServices = authenticationServices;
            var authResult = authenticationServices.AuthenticationResult;
            var activeDirectoryCred = new ActiveDirectoryCredentials(authResult.AccessToken);
            this.azureResourceManagerClient = new AzureResourceManagerClient(activeDirectoryCred);
            this.GetSubscriptionsAsync();
            
        }

        #region Binded Properties

        public List<string> Subscriptions
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

        public string SelectedSubscription
        {
            get
            {
                return this.selectedSubscription;
            }

            set
            {
                this.selectedSubscription = value;
                //MessageBox.Show("selected sub is: " + value);
                this.SelectedResourceGroup = null;
                this.SelectedResourceType = null;
                this.SelectedResource = null;
                this.OnPropertyChanged();
                this.GetResourceGroupsAsync();
            }
        }

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

        public string SelectedResourceGroup
        {
            get
            {
                return this.selectedResourceGroup;
            }

            set
            {
                this.selectedResourceGroup = value;
                //MessageBox.Show("selected resource group is: " + value);                
                this.OnPropertyChanged();
                this.SelectedResourceType = null;
                this.SelectedResource = null;

                if (value != null)
                {
                    this.GetResourcesTypesAsync();
                }
            }
        }

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

        public string SelectedResourceType
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResourceType = value;
                //MessageBox.Show("selected resource group is: " + value);
                this.OnPropertyChanged();
                this.SelectedResource = null;

                if (value != null)
                {
                    ResourceType selectedResourceType = (ResourceType)Enum.Parse(typeof(ResourceType), this.SelectedResourceType);
                    this.Resources = this.resourcesIndentifiers.Where(resourceIndentifier => resourceIndentifier.ResourceType == selectedResourceType)
                                        .Select(resourceIndentifier => resourceIndentifier.ResourceName).ToList();
                }
            }
        }

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

        public string SelectedResource
        {
            get
            {
                return this.selectedResourceType;
            }

            set
            {
                this.selectedResource = value;
                //MessageBox.Show("selected resource is: " + value);
                this.OnPropertyChanged();
            }
        }

        #endregion

        private async void GetSubscriptionsAsync()
        {
            try
            {
                this.Subscriptions = (List<string>) await this.azureResourceManagerClient.GetAllSubscriptionIdsAsync();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        private async void GetResourceGroupsAsync()
        {
            try
            {
                var groupsIndentifiers = (await this.azureResourceManagerClient.GetAllResourceGroupsInSubscriptionAsync(SelectedSubscription, CancellationToken.None)).ToList();
                this.ResourceGroups = groupsIndentifiers.Select(ri => ri.ResourceGroupName).ToList();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        private async void GetResourcesTypesAsync()
        {
            try
            {
                var supportedResourceTypes = new List<ResourceType>() { ResourceType.ApplicationInsights, ResourceType.LogAnalytics, ResourceType.VirtualMachine };
                this.resourcesIndentifiers = (await this.azureResourceManagerClient.GetAllResourcesInResourceGroupAsync(SelectedSubscription, SelectedResourceGroup, supportedResourceTypes, CancellationToken.None)).ToList();
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
