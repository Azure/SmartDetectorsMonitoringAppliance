//-----------------------------------------------------------------------
// <copyright file="ResourceType.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    /// <summary>
    /// An enumeration of all resource types supported by Smart Signals.
    /// </summary>
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
    }
}
