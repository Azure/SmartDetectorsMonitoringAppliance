//-----------------------------------------------------------------------
// <copyright file="AlertExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Newtonsoft.Json;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using AlertState = Microsoft.Azure.Monitoring.SmartDetectors.AlertState;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsAlertState = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertState;
    using ContractsChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartAxisType;
    using ContractsChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartPoint;
    using ContractsChartType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.ChartType;

    /// <summary>
    /// A class for Alert extension methods
    /// </summary>
    public static class AlertExtensions
    {
        /// <summary>
        /// Creates a presentation from an alert
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="request">The Smart Detector request</param>
        /// <param name="smartDetectorName">The Smart Detector name</param>
        /// <param name="queryRunInfo">The query run information</param>
        /// <param name="usedLogAnalysisClient">Indicates whether a log analysis client was used to create the alert</param>
        /// <param name="usedMetricClient">Indicates whether a metric client was used to create the alert</param>
        /// <returns>The presentation</returns>
        public static ContractsAlert CreateContractsAlert(this Alert alert, SmartDetectorExecutionRequest request, string smartDetectorName, QueryRunInfo queryRunInfo, bool usedLogAnalysisClient, bool usedMetricClient)
        {
            // A null alert has null presentation
            if (alert == null)
            {
                return null;
            }

            // Create presentation elements for each alert property
            Dictionary<string, string> predicates = new Dictionary<string, string>();
            #pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
            List<AlertPropertyLegacy> alertPropertiesLegacy = new List<AlertPropertyLegacy>();
            #pragma warning restore CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
            List<AlertProperty> alertProperties = new List<AlertProperty>();
            Dictionary<string, string> rawProperties = new Dictionary<string, string>();
            List<string> alertBaseClassPropertiesNames = typeof(Alert).GetProperties().Select(p => p.Name).ToList();
            foreach (PropertyInfo property in alert.GetType().GetProperties())
            {
                // Get the property value
                object propertyValue = property.GetValue(alert);
                string propertyStringValue = PropertyValueToString(propertyValue);
                if (string.IsNullOrWhiteSpace(propertyStringValue) || (propertyValue is ICollection value && value.Count == 0))
                {
                    // not accepting empty properties
                    continue;
                }

                rawProperties[property.Name] = propertyStringValue;

                // Check if this property is a predicate
                if (property.GetCustomAttribute<AlertPredicatePropertyAttribute>() != null)
                {
                    predicates[property.Name] = propertyStringValue;
                }

                // Get the v1 presentation attribute
                AlertPresentationPropertyAttribute presentationAttribute = property.GetCustomAttribute<AlertPresentationPropertyAttribute>();
                if (presentationAttribute != null)
                {
                    alertPropertiesLegacy.Add(CreateAlertPropertyLegacy(alert, presentationAttribute, queryRunInfo, propertyStringValue));
                }

                // Get the v2 presentation attribute
                AlertPresentationPropertyV2Attribute presentationV2Attribute = property.GetCustomAttribute<AlertPresentationPropertyV2Attribute>();
                if (presentationV2Attribute != null)
                {
                    alertProperties.Add(CreateAlertProperty(alert, presentationV2Attribute, property.Name, propertyValue));
                }
                else if (!alertBaseClassPropertiesNames.Contains(property.Name))
                {
                    // Get the raw alert property - a property with no presentation
                    alertProperties.Add(new RawAlertProperty(property.Name, propertyValue));
                }
            }

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).ToSha256Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", predicates.OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value)).ToSha256Hash();

            // Get the alert's signal type based on the clients used to create the alert
            SignalType signalType = GetSignalType(usedLogAnalysisClient, usedMetricClient);

            // Return the presentation object
            #pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
            return new ContractsAlert
            {
                Id = id,
                State = (alert.State == AlertState.Active) ? ContractsAlertState.Active : ContractsAlertState.Resolved,
                Title = alert.Title,
                ResourceId = resourceId,
                CorrelationHash = correlationHash,
                SmartDetectorId = request.SmartDetectorId,
                SmartDetectorName = smartDetectorName,
                AnalysisTimestamp = DateTime.UtcNow,
                AnalysisWindowSizeInMinutes = (int)request.Cadence.TotalMinutes,
                Properties = alertPropertiesLegacy,
                AlertProperties = alertProperties,
                RawProperties = rawProperties,
                QueryRunInfo = queryRunInfo,
                SignalType = signalType
            };
            #pragma warning restore CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
        }

        /// <summary>
        /// Creates an <see cref="AlertPropertyLegacy"/> based on an alert presentation V1 property
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="presentationAttribute">The attribute defining the presentation V1 of the alert property</param>
        /// <param name="queryRunInfo">The query run information</param>
        /// <param name="propertyStringValue">The property string value</param>
        /// <returns>An <see cref="AlertPropertyLegacy"/></returns>
#pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
        private static AlertPropertyLegacy CreateAlertPropertyLegacy(Alert alert, AlertPresentationPropertyAttribute presentationAttribute, QueryRunInfo queryRunInfo, string propertyStringValue)
        {
            // Verify that if the entity is a chart or query, then query run information was provided
            if (queryRunInfo == null && (presentationAttribute.Section == AlertPresentationSection.Chart || presentationAttribute.Section == AlertPresentationSection.AdditionalQuery))
            {
                throw new InvalidAlertPresentationException($"The presentation contains an item for the {presentationAttribute.Section} section, but no telemetry data client was provided");
            }

            // Get the attribute title and information balloon - support interpolated strings
            string attributeTitle = presentationAttribute.Title.EvaluateInterpolatedString(alert);
            string attributeInfoBalloon = presentationAttribute.InfoBalloon.EvaluateInterpolatedString(alert);

            // Add the presentation property
            return new AlertPropertyLegacy()
            {
                Name = attributeTitle,
                Value = propertyStringValue,
                DisplayCategory = GetDisplayCategoryFromPresentationSection(presentationAttribute.Section),
                InfoBalloon = attributeInfoBalloon,
                Order = presentationAttribute.Order
            };
        }
        #pragma warning restore CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924

        /// <summary>
        /// Creates an <see cref="AlertProperty"/> based on an alert presentation V2 property
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="presentationAttribute">The attribute defining the presentation V2 of the alert property</param>
        /// <param name="propertyDefaultName">The property default name</param>
        /// <param name="propertyValue">The property value</param>
        /// <returns>An <see cref="AlertProperty"/></returns>
        private static AlertProperty CreateAlertProperty(Alert alert, AlertPresentationPropertyV2Attribute presentationAttribute, string propertyDefaultName, object propertyValue)
        {
            // Get the attribute display name
            string displayName = presentationAttribute.DisplayName.EvaluateInterpolatedString(alert);

            // Get the property name
            string propertyName = string.IsNullOrWhiteSpace(presentationAttribute.PropertyName) ? propertyDefaultName : presentationAttribute.PropertyName;

            // Return the presentation property according to the property type
            switch (presentationAttribute)
            {
                case AlertPresentationChartAttribute chartAttribute:
                    if (!(propertyValue is IList<ChartPoint> listValues))
                    {
                        throw new ArgumentException("An AlertPresentationChartAttribute can only be applied to properties of type IList<ChartPoint>");
                    }

                    return new ChartAlertProperty(
                        propertyName,
                        displayName,
                        presentationAttribute.Order,
                        ConvertChartTypeToContractsChartType(chartAttribute.ChartType),
                        ConvertChartAxisTypeToContractsChartType(chartAttribute.XAxisType),
                        ConvertChartAxisTypeToContractsChartType(chartAttribute.YAxisType),
                        listValues.Select(point => new ContractsChartPoint(point.X, point.Y)).ToList());

                case AlertPresentationLongTextAttribute longTextAttribute:
                    return new LongTextAlertProprety(propertyName, displayName, presentationAttribute.Order, PropertyValueToString(propertyValue));

                case AlertPresentationTextAttribute textAttribute:
                    return new TextAlertProperty(propertyName, displayName, presentationAttribute.Order, PropertyValueToString(propertyValue));

                case AlertPresentationUrlAttribute urlAttribute:
                    if (!(propertyValue is Uri uriValue))
                    {
                        throw new ArgumentException("An AlertPresentationUrlAttribute can only be applied to properties of type Uri");
                    }

                    if (!uriValue.IsAbsoluteUri)
                    {
                        throw new ArgumentException("The URI supplied must be absolute");
                    }

                    string linkText = urlAttribute.LinkText.EvaluateInterpolatedString(alert);
                    return new TextAlertProperty(propertyName, displayName, presentationAttribute.Order, $"<a href=\"{uriValue.ToString()}\">{linkText}</a>");

                case AlertPresentationKeyValueAttribute keyValueAttribute:
                    if (!(propertyValue is IDictionary<string, string> keyValuePropertyValue))
                    {
                        throw new ArgumentException("An AlertPresentationKeyValueAttribute can only be applied to properties of type IDictionary<string, string>");
                    }

                    if (keyValueAttribute.ShowHeaders)
                    {
                        string keyHeaderName = keyValueAttribute.KeyHeaderName.EvaluateInterpolatedString(alert);
                        string valueHeaderName = keyValueAttribute.ValueHeaderName.EvaluateInterpolatedString(alert);
                        return new KeyValueAlertProperty(propertyName, displayName, presentationAttribute.Order, keyHeaderName, valueHeaderName, keyValuePropertyValue);
                    }
                    else
                    {
                        return new KeyValueAlertProperty(propertyName, displayName, presentationAttribute.Order, keyValuePropertyValue);
                    }

                case AlertPresentationSingleColumnTableAttribute singleColumnTableAttribute:
                    if (!(propertyValue is IList singleColumnTablePropertyValue))
                    {
                        throw new ArgumentException("An AlertPresentationSingleColumnTableAttribute can only be applied to properties of type IList");
                    }

                    return new TableAlertProperty(propertyName, displayName, presentationAttribute.Order, singleColumnTableAttribute.ShowHeaders, singleColumnTablePropertyValue);

                case AlertPresentationTableAttribute tableAttribute:
                    if (!(propertyValue is IList tablePropertyValue))
                    {
                        throw new ArgumentException("An AlertPresentationTableAttribute can only be applied to properties of type IList");
                    }

                    Type tableRowType = GetGenericListType(propertyValue.GetType());
                    if (tableRowType == null)
                    {
                        throw new ArgumentException("An AlertPresentationTableAttribute can only be applied to properties of type IList<>");
                    }

                    return new TableAlertProperty(propertyName, displayName, presentationAttribute.Order, tableAttribute.ShowHeaders, CreateTableColumnsFromRowType(tableRowType), tablePropertyValue);

                default:
                    throw new InvalidEnumArgumentException($"Unable to handle presentation attribute of type {presentationAttribute.GetType().Name}");
            }
        }

        /// <summary>
        /// Gets the <see cref="SignalType"/> based on the clients used to create the alert
        /// </summary>
        /// <param name="usedLogAnalysisClient">Indicates whether a log analysis client was used to create the alert</param>
        /// <param name="usedMetricClient">Indicates whether a metric client was used to create the alert</param>
        /// <returns>A <see cref="SignalType"/> based on the clients used to create the alert</returns>
        private static SignalType GetSignalType(bool usedLogAnalysisClient, bool usedMetricClient)
        {
            if (usedMetricClient)
            {
                return usedLogAnalysisClient ? SignalType.Multiple : SignalType.Metric;
            }

            return SignalType.Log;
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
                return ((DateTime)propertyValue).ToString("u", CultureInfo.InvariantCulture);
            }
            else
            {
                return propertyValue.ToString();
            }
        }

        /// <summary>
        /// Gets the display category enum value from the presentation section enum value
        /// </summary>
        /// <param name="presentationSection">The property presentation section</param>
        /// <returns>The display category that coralline with the presentation section</returns>
#pragma warning disable 612 // Task to remove obsolete code #1312924
        private static AlertPropertyDisplayCategory GetDisplayCategoryFromPresentationSection(AlertPresentationSection presentationSection)
        {
            switch (presentationSection)
            {
                case AlertPresentationSection.AdditionalQuery:
                    return AlertPropertyDisplayCategory.AdditionalQuery;

                case AlertPresentationSection.Analysis:
                    return AlertPropertyDisplayCategory.Analysis;

                case AlertPresentationSection.Chart:
                    return AlertPropertyDisplayCategory.Chart;

                case AlertPresentationSection.Property:
                default:
                    return AlertPropertyDisplayCategory.Property;
            }
        }
#pragma warning restore 612

        /// <summary>
        /// Converts chart type to contracts chart type
        /// </summary>
        /// <param name="chartType">The chart type to convert</param>
        /// <returns>Contracts chart type</returns>
        private static ContractsChartType ConvertChartTypeToContractsChartType(ChartType chartType)
        {
            switch (chartType)
            {
                case ChartType.BarChart:
                    return ContractsChartType.BarChart;
                case ChartType.LineChart:
                    return ContractsChartType.LineChart;
                default:
                    throw new InvalidEnumArgumentException("Chart type can be Bar or Line only");
            }
        }

        /// <summary>
        /// Converts chart axis type to contracts chart axis type
        /// </summary>
        /// <param name="chartAxisType">The chart axis type to convert</param>
        /// <returns>Contracts chart axis type</returns>
        private static ContractsChartAxisType ConvertChartAxisTypeToContractsChartType(ChartAxisType chartAxisType)
        {
            switch (chartAxisType)
            {
                case ChartAxisType.NumberAxis:
                    return ContractsChartAxisType.Number;
                case ChartAxisType.DateAxis:
                    return ContractsChartAxisType.Date;
                case ChartAxisType.StringAxis:
                    return ContractsChartAxisType.String;
                default:
                    throw new InvalidEnumArgumentException("Chart type can be Number, Date or String only");
            }
        }

        /// <summary>
        /// Extracts the list of table columns from the type of the table row, by looking for properties
        /// with attribute <see cref="AlertPresentationTableColumnAttribute"/>.
        /// </summary>
        /// <param name="tableRowType">The table row type.</param>
        /// <returns>A list of <see cref="TableColumn"/> objects describing the row type.</returns>
        private static List<TableColumn> CreateTableColumnsFromRowType(Type tableRowType)
        {
            var columns = new List<TableColumn>();
            foreach (PropertyInfo property in tableRowType.GetProperties())
            {
                // Check if this property is a table column
                AlertPresentationTableColumnAttribute tableColumnAttribute = property.GetCustomAttribute<AlertPresentationTableColumnAttribute>();
                if (tableColumnAttribute != null)
                {
                    string propertyName = property.Name;
                    JsonPropertyAttribute jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                    if (jsonPropertyAttribute != null)
                    {
                        propertyName = jsonPropertyAttribute.PropertyName;
                    }

                    columns.Add(new TableColumn(propertyName, tableColumnAttribute.DisplayName));
                }
            }

            return columns;
        }

        /// <summary>
        /// Checks if <paramref name="type"/> implements the <see cref="IList{T}"/> interface, and if so returns
        /// the element type of that list. Otherwise returns <c>null</c>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The element type of the list, or <c>null</c> if <paramref name="type"/> is not a list.</returns>
        private static Type GetGenericListType(Type type)
        {
            Type genericListInterface = type.GetInterfaces().Where(i => i.IsGenericType).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IList<>));
            return genericListInterface?.GetGenericArguments().Single();
        }
    }
}