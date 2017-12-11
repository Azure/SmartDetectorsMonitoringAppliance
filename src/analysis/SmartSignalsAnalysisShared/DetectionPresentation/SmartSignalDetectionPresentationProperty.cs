namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis.DetectionPresentation
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class holds presentation information of a single detection property
    /// </summary>
    public class SmartSignalDetectionPresentationProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalDetectionPresentationProperty"/> class
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        /// <param name="displayCategory">The property display category</param>
        /// <param name="infoBalloon">The property information balloon</param>
        public SmartSignalDetectionPresentationProperty(string name, string value, DetectionPresentationSection displayCategory, string infoBalloon)
        {
            this.Name = name;
            this.Value = value;
            this.DisplayCategory = displayCategory;
            this.InfoBalloon = infoBalloon;
        }

        /// <summary>
        /// Gets the property name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Gets the property value
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; }

        /// <summary>
        /// Gets the property display category
        /// </summary>
        [JsonProperty("displayCategory")]
        public DetectionPresentationSection DisplayCategory { get; }

        /// <summary>
        /// Gets the property information balloon
        /// </summary>
        [JsonProperty("infoBalloon")]
        public string InfoBalloon { get; }
    }
}