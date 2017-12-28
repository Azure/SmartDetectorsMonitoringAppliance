//-----------------------------------------------------------------------
// <copyright file="SignalConfiguration.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents a signal configuration for the Management API responses.
    /// </summary>
    public class SignalConfiguration
    {
        /// <summary>
        /// Gets or sets the signal configuration id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the signal configuration name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the signal configuration type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
