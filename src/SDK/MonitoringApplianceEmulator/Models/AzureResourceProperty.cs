//-----------------------------------------------------------------------
// <copyright file="AzureResourceProperty.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    /// <summary>
    /// Represents an Azure resource property (e.g. subscription. resource group).
    /// </summary>
    public class AzureResourceProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceProperty"/> class.
        /// </summary>
        /// <param name="resourceType">The display string of the resource type</param>
        /// <param name="resourceName">The display string of the resource</param>
        public AzureResourceProperty(string resourceType, string resourceName)
        {
            this.ResourceType = resourceType;
            this.ResourceName = resourceName;
        }

        /// <summary>
        /// Gets the display string of the resource type.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the display string of the resource.
        /// </summary>
        public string ResourceName { get; }
    }
}