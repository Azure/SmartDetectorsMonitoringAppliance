namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.DetectionPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Extensions;
    using Newtonsoft.Json;
    using SmartFormat;

    /// <summary>
    /// This class holds the presentation information of the detection -
    /// the way a detection should be presented in the UI
    /// </summary>
    public class SmartSignalDetectionPresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalDetectionPresentation"/> class
        /// </summary>
        /// <param name="id">The detection ID</param>
        /// <param name="title">The detection title</param>
        /// <param name="summary">The detection summary</param>
        /// <param name="resourceId">The detection resource ID</param>
        /// <param name="correlationHash">The detection correlation hash</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="analysisTimestamp">The end time of the analysis window</param>
        /// <param name="analysisWindowSizeInMinutes">The analysis window size (in minutes)</param>
        /// <param name="properties">The detection properties</param>
        /// <param name="rawProperties">The raw detection properties</param>
        public SmartSignalDetectionPresentation(string id, string title, SmartSignalDetectionPresentationSummary summary, string resourceId, string correlationHash, string signalId, string signalName, DateTime analysisTimestamp, int analysisWindowSizeInMinutes, List<SmartSignalDetectionPresentationProperty> properties, IReadOnlyDictionary<string, string> rawProperties)
        {
            this.Id = id;
            this.Title = title;
            this.Summary = summary;
            this.ResourceId = resourceId;
            this.CorrelationHash = correlationHash;
            this.SignalId = signalId;
            this.SignalName = signalName;
            this.AnalysisTimestamp = analysisTimestamp;
            this.AnalysisWindowSizeInMinutes = analysisWindowSizeInMinutes;
            this.Properties = properties;
            this.RawProperties = rawProperties;
        }

        /// <summary>
        /// Gets the detection ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the detection title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the detection summary
        /// </summary>
        [JsonProperty("summary")]
        public SmartSignalDetectionPresentationSummary Summary { get; }

        /// <summary>
        /// Gets the detection resource ID
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; }

        /// <summary>
        /// Gets the detection correlation hash
        /// </summary>
        [JsonProperty("correlationHash")]
        public string CorrelationHash { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        [JsonProperty("signalId")]
        public string SignalId { get; }

        /// <summary>
        /// Gets the signal name
        /// </summary>
        [JsonProperty("signalName")]
        public string SignalName { get; }

        /// <summary>
        /// Gets the end time of the analysis window
        /// </summary>
        [JsonProperty("analysisTimestamp")]
        public DateTime AnalysisTimestamp { get; }

        /// <summary>
        /// Gets the analysis window size (in minutes)
        /// </summary>
        [JsonProperty("analysisWindowSizeInMinutes")]
        public int AnalysisWindowSizeInMinutes { get; }

        /// <summary>
        /// Gets the detection properties
        /// </summary>
        [JsonProperty("properties")]
        public List<SmartSignalDetectionPresentationProperty> Properties { get; }

        /// <summary>
        /// Gets the raw detection properties
        /// </summary>
        [JsonProperty("rawProperties")]
        public IReadOnlyDictionary<string, string> RawProperties { get; }

        /// <summary>
        /// Creates a presentation from a detection
        /// </summary>
        /// <param name="request">The smart signal request</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="smartSignalDetection">The detection</param>
        /// <returns>The presentation</returns>
        public static SmartSignalDetectionPresentation CreateFromDetection(SmartSignalRequest request, string signalName, SmartSignalDetection smartSignalDetection)
        {
            // A null detection has null presentation
            if (smartSignalDetection == null)
            {
                return null;
            }

            // Create presentation elements for each detection property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            List<SmartSignalDetectionPresentationProperty> properties = new List<SmartSignalDetectionPresentationProperty>();
            SmartSignalDetectionPresentationProperty summaryChart = null;
            string summaryValue = null;
            string summaryDetails = null;
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            foreach (PropertyInfo property in smartSignalDetection.GetType().GetProperties())
            {
                // Get the property value
                string propertyValue = PropertyValueToString(property.GetValue(smartSignalDetection));
                rawProperties[property.Name] = propertyValue;

                // Check if this property is a predicate
                if (property.GetCustomAttribute<DetectionPredicateAttribute>() != null)
                {
                    predicates[property.Name] = propertyValue;
                }

                // Get the presentation attribute
                DetectionPresentationAttribute attribute = property.GetCustomAttribute<DetectionPresentationAttribute>();
                if (attribute != null)
                {
                    // Get the attribute title and information balloon - support interpolated strings
                    string attributeTitle = Smart.Format(attribute.Title, smartSignalDetection);
                    string attributeInfoBalloon = Smart.Format(attribute.InfoBalloon, smartSignalDetection);

                    // Add presentation to the summary component
                    if (attribute.Component.HasFlag(DetectionPresentationComponent.Summary))
                    {
                        if (attribute.Section == DetectionPresentationSection.Chart)
                        {
                            // Verify there is at most one summary chart
                            if (summaryChart != null)
                            {
                                throw new InvalidDetectionPresentationException("There can be at most one summary chart for each detection");
                            }

                            // Create the summary chart presentation property
                            summaryChart = new SmartSignalDetectionPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon);
                        }
                        else if (attribute.Section == DetectionPresentationSection.Property)
                        {
                            // Verify there is at most one summary presentation property
                            if (summaryValue != null)
                            {
                                throw new InvalidDetectionPresentationException("There must be exactly one summary property for each detection");
                            }

                            // Set summary presentation elements
                            summaryValue = propertyValue;
                            summaryDetails = attributeTitle;
                        }
                        else
                        {
                            throw new InvalidDetectionPresentationException($"Invalid section for summary property {property.Name}: {attribute.Section}");
                        }
                    }

                    // Add presentation to the details component
                    if (attribute.Component.HasFlag(DetectionPresentationComponent.Details))
                    {
                        properties.Add(new SmartSignalDetectionPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon));
                    }
                }
            }

            // Verify that a summary was provided
            if (summaryValue == null)
            {
                throw new InvalidDetectionPresentationException("There must be exactly one summary property for each detection");
            }

            string id = string.Join("##", smartSignalDetection.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(smartSignalDetection)).Hash();
            string resourceId = string.Empty; // TODO: add resource ID to every detection
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).Hash();

            // Return the presentation object
            return new SmartSignalDetectionPresentation(
                id,
                smartSignalDetection.Title,
                new SmartSignalDetectionPresentationSummary(summaryValue, summaryDetails, summaryChart),
                resourceId,
                correlationHash,
                request.SignalId,
                signalName,
                request.AnalysisEndTime,
                (int)(request.AnalysisEndTime - request.AnalysisStartTime).TotalMinutes,
                properties,
                rawProperties);
        }

        /// <summary>
        /// Converts a presentation property's value to a string
        /// </summary>
        /// <param name="propertyValue">The property value</param>
        /// <returns>The string</returns>
        private static string PropertyValueToString(object propertyValue)
        {
            if (propertyValue == null)
            {
                // null is an empty string
                return string.Empty;
            }
            else if (propertyValue is DateTime)
            {
                // Convert to universal sortable time
                return ((DateTime)propertyValue).ToString("u");
            }
            else
            {
                return propertyValue.ToString();
            }
        }
    }
}