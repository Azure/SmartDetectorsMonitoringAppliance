//-----------------------------------------------------------------------
// <copyright file="AzureSubscription.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    /// <summary>
    /// Represents an Azure subscription.
    /// </summary>
    public class AzureSubscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSubscription"/> class.
        /// </summary>
        /// <param name="id">The subscription id</param>
        /// <param name="displayName">The subscription display name</param>
        public AzureSubscription(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Gets the subscription id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the subscription display name.
        /// </summary>
        public string DisplayName { get; }
    }
}