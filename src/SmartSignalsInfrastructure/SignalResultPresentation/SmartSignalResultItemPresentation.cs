//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultItemPresentation.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.SignalResultPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.Extensions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Newtonsoft.Json;
    using SmartFormat;

    /// <summary>
    /// This class holds the presentation information of the result item -
    /// the way a result item should be presented in the UI
    /// </summary>
    public class SmartSignalResultItemPresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultItemPresentation"/> class
        /// </summary>
        /// <param name="id">The result item ID</param>
        /// <param name="title">The result item title</param>
        /// <param name="summary">The result item summary</param>
        /// <param name="resourceId">The result item resource ID</param>
        /// <param name="correlationHash">The result item correlation hash</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="analysisTimestamp">The end time of the analysis window</param>
        /// <param name="analysisWindowSizeInMinutes">The analysis window size (in minutes)</param>
        /// <param name="properties">The result item properties</param>
        /// <param name="rawProperties">The raw result item properties</param>
        public SmartSignalResultItemPresentation(string id, string title, SmartSignalResultItemPresentationSummary summary, string resourceId, string correlationHash, string signalId, string signalName, DateTime analysisTimestamp, int analysisWindowSizeInMinutes, List<SmartSignalResultItemPresentationProperty> properties, IReadOnlyDictionary<string, string> rawProperties)
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
        /// Gets the result item ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the result item title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the result item summary
        /// </summary>
        [JsonProperty("summary")]
        public SmartSignalResultItemPresentationSummary Summary { get; }

        /// <summary>
        /// Gets the result item resource ID
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; }

        /// <summary>
        /// Gets the result item correlation hash
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
        /// Gets the result item properties
        /// </summary>
        [JsonProperty("properties")]
        public List<SmartSignalResultItemPresentationProperty> Properties { get; }

        /// <summary>
        /// Gets the raw result item properties
        /// </summary>
        [JsonProperty("rawProperties")]
        public IReadOnlyDictionary<string, string> RawProperties { get; }

        /// <summary>
        /// Creates a presentation from a result item
        /// </summary>
        /// <param name="request">The smart signal request</param>
        /// <param name="signalName">The signal name</param>
        /// <param name="smartSignalResultItem">The result item</param>
        /// <param name="azureResourceManagerClient">The azure resource manager client</param>
        /// <returns>The presentation</returns>
        public static SmartSignalResultItemPresentation CreateFromResultItem(SmartSignalRequest request, string signalName, SmartSignalResultItem smartSignalResultItem, IAzureResourceManagerClient azureResourceManagerClient)
        {
            // A null result item has null presentation
            if (smartSignalResultItem == null)
            {
                return null;
            }

            // Create presentation elements for each result item property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            List<SmartSignalResultItemPresentationProperty> properties = new List<SmartSignalResultItemPresentationProperty>();
            SmartSignalResultItemPresentationProperty summaryChart = null;
            string summaryValue = null;
            string summaryDetails = null;
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            foreach (PropertyInfo property in smartSignalResultItem.GetType().GetProperties())
            {
                // Get the property value
                string propertyValue = PropertyValueToString(property.GetValue(smartSignalResultItem));
                rawProperties[property.Name] = propertyValue;

                // Check if this property is a predicate
                if (property.GetCustomAttribute<ResultItemPredicateAttribute>() != null)
                {
                    predicates[property.Name] = propertyValue;
                }

                // Get the presentation attribute
                ResultItemPresentationAttribute attribute = property.GetCustomAttribute<ResultItemPresentationAttribute>();
                if (attribute != null)
                {
                    // Get the attribute title and information balloon - support interpolated strings
                    string attributeTitle = Smart.Format(attribute.Title, smartSignalResultItem);
                    string attributeInfoBalloon = Smart.Format(attribute.InfoBalloon, smartSignalResultItem);

                    // Add presentation to the summary component
                    if (attribute.Component.HasFlag(ResultItemPresentationComponent.Summary))
                    {
                        if (attribute.Section == ResultItemPresentationSection.Chart)
                        {
                            // Verify there is at most one summary chart
                            if (summaryChart != null)
                            {
                                throw new InvalidSmartSignalResultItemPresentationException("There can be at most one summary chart for each resultItem");
                            }

                            // Create the summary chart presentation property
                            summaryChart = new SmartSignalResultItemPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon);
                        }
                        else if (attribute.Section == ResultItemPresentationSection.Property)
                        {
                            // Verify there is at most one summary presentation property
                            if (summaryValue != null)
                            {
                                throw new InvalidSmartSignalResultItemPresentationException("There must be exactly one summary property for each resultItem");
                            }

                            // Set summary presentation elements
                            summaryValue = propertyValue;
                            summaryDetails = attributeTitle;
                        }
                        else
                        {
                            throw new InvalidSmartSignalResultItemPresentationException($"Invalid section for summary property {property.Name}: {attribute.Section}");
                        }
                    }

                    // Add presentation to the details component
                    if (attribute.Component.HasFlag(ResultItemPresentationComponent.Details))
                    {
                        properties.Add(new SmartSignalResultItemPresentationProperty(attributeTitle, propertyValue, attribute.Section, attributeInfoBalloon));
                    }
                }
            }

            // Verify that a summary was provided
            if (summaryValue == null)
            {
                throw new InvalidSmartSignalResultItemPresentationException("There must be exactly one summary property for each result item");
            }

            string id = string.Join("##", smartSignalResultItem.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(smartSignalResultItem)).Hash();
            string resourceId = azureResourceManagerClient.GetResourceId(smartSignalResultItem.ResourceIdentifier);
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).Hash();

            // Return the presentation object
            return new SmartSignalResultItemPresentation(
                id,
                smartSignalResultItem.Title,
                new SmartSignalResultItemPresentationSummary(summaryValue, summaryDetails, summaryChart),
                resourceId,
                correlationHash,
                request.SignalId,
                signalName,
                DateTime.UtcNow,
                (int)request.Cadence.TotalMinutes,
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