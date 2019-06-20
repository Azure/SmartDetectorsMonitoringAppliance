//-----------------------------------------------------------------------
// <copyright file="ResourceType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of all resource types supported by Smart Detectors.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceType
    {
        /// <summary>
        /// The Subscription resource type.
        /// </summary>
        Subscription,

        /// <summary>
        /// The Resource Group resource type.
        /// </summary>
        ResourceGroup,

        /// <summary>
        /// The Virtual Machine resource type.
        /// </summary>
        VirtualMachine,

        /// <summary>
        /// The Virtual Machine Scale Set resource type.
        /// </summary>
        VirtualMachineScaleSet,

        /// <summary>
        /// The Application Instance resource type.
        /// </summary>
        ApplicationInsights,

        /// <summary>
        /// The Log Analytics Workspace resource type.
        /// </summary>
        LogAnalytics,

        /// <summary>
        /// The Azure Storage resource type.
        /// </summary>
        AzureStorage,

        /// <summary>
        /// The Azure Cosmos DB resource type.
        /// </summary>
        CosmosDb,

        /// <summary>
        /// The Azure Key Vault resource type.
        /// </summary>
        KeyVault,

        /// <summary>
        /// The Azure Service Bus resource type.
        /// </summary>
        ServiceBus,

        /// <summary>
        /// The Azure SQL Server resource type.
        /// </summary>
        SqlServer,

        /// <summary>
        /// The Azure Event Hub resource type
        /// </summary>
        EventHub,

        /// <summary>
        /// The Azure Web Site resource type
        /// </summary>
        WebSite,

        /// <summary>
        /// The Logic Apps resource type
        /// </summary>
        LogicApps,

        /// <summary>
        /// The Azure Kuberneties Service resource type
        /// </summary>
        KubernetiesService,
    }
}
