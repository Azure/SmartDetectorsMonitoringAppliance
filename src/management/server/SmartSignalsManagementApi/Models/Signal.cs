namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents a signal model for the Management API responses.
    /// </summary>
    public class Signal
    {
        /// <summary>
        /// Gets or sets the signal id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the signal name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the signal supported cadences (in minutes).
        /// </summary>
        [JsonProperty("supportedCadences")]
        public List<int> SupportedCadences { get; set; }

        /// <summary>
        /// Gets or sets the signal configurations.
        /// </summary>
        [JsonProperty("configurations")]
        public List<SignalConfiguration> Configurations { get; set; }
    }
}
