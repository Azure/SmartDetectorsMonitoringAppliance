//-----------------------------------------------------------------------
// <copyright file="AlertExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Newtonsoft.Json;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
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
        public static ContractsAlert CreateContractsAlert(this Alert alert, SmartDetectorAnalysisRequest request, string smartDetectorName, QueryRunInfo queryRunInfo, bool usedLogAnalysisClient, bool usedMetricClient)
        {
            // A null alert has null presentation
            if (alert == null)
            {
                return null;
            }

            // Create presentation elements for each alert property
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
                string propertyStringValue = PropertyValueToString(alert, property, propertyValue);
                if (string.IsNullOrWhiteSpace(propertyStringValue) || (propertyValue is ICollection value && value.Count == 0))
                {
                    // not accepting empty properties
                    continue;
                }

                rawProperties[property.Name] = propertyStringValue;

                // Get the presentation attribute
                AlertPresentationPropertyV2Attribute presentationV2Attribute = property.GetCustomAttribute<AlertPresentationPropertyV2Attribute>();
                if (presentationV2Attribute != null)
                {
                    alertProperties.Add(CreateAlertProperty(alert, property, presentationV2Attribute, propertyValue));
                }
                else if (!alertBaseClassPropertiesNames.Contains(property.Name))
                {
                    // Get the raw alert property - a property with no presentation
                    alertProperties.Add(new RawAlertProperty(property.Name, propertyValue));
                }
            }

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).ToSha256Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", alert.ExtractPredicates().OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value.ToString())).ToSha256Hash();

            // Get the alert's signal type based on the clients used to create the alert
            SignalType signalType = GetSignalType(usedLogAnalysisClient, usedMetricClient);

            // Return the presentation object
            #pragma warning disable CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
            return new ContractsAlert
            {
                Id = id,
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
                SignalType = signalType,
                AutomaticResolutionParameters = alert.AutomaticResolutionParameters?.CreateContractsAutomaticResolutionParameters()
            };
            #pragma warning restore CS0612 // Type or member is obsolete; Task to remove obsolete code #1312924
        }

        /// <summary>
        /// Extracts the predicate properties from the alert. Predicate properties are properties
        /// in the alert's object which are marked by <see cref="PredicatePropertyAttribute"/>.
        /// </summary>
        /// <param name="alert">The alert to extract the predicates from.</param>
        /// <returns>A dictionary mapping each predicate property name to its value.</returns>
        public static Dictionary<string, object> ExtractPredicates(this Alert alert)
        {
            if (alert == null)
            {
                throw new ArgumentNullException(nameof(alert));
            }

            var predicates = new Dictionary<string, object>();
            foreach (PropertyInfo property in alert.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<PredicatePropertyAttribute>() != null))
            {
                predicates[property.Name] = property.GetValue(alert);
            }

            return predicates;
        }

        /// <summary>
        /// Creates an <see cref="AlertProperty"/> based on an alert presentation V2 property
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="property">The property info of the property to create</param>
        /// <param name="presentationAttribute">The attribute defining the presentation V2 of the alert property</param>
        /// <param name="propertyValue">The property value</param>
        /// <returns>An <see cref="AlertProperty"/></returns>
        private static AlertProperty CreateAlertProperty(Alert alert, PropertyInfo property, AlertPresentationPropertyV2Attribute presentationAttribute, object propertyValue)
        {
            // Get the attribute display name
            string displayName = presentationAttribute.DisplayName.EvaluateInterpolatedString(alert);

            // Get the property name
            string propertyName = string.IsNullOrWhiteSpace(presentationAttribute.PropertyName) ? property.Name : presentationAttribute.PropertyName;

            // Return the presentation property according to the property type
            switch (presentationAttribute)
            {
                case ChartPropertyAttribute chartAttribute:
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

                case LongTextPropertyAttribute _:
                    return new LongTextAlertProprety(propertyName, displayName, presentationAttribute.Order, PropertyValueToString(alert, property, propertyValue));

                case TextPropertyAttribute _:
                    return new TextAlertProperty(propertyName, displayName, presentationAttribute.Order, PropertyValueToString(alert, property, propertyValue));

                case KeyValuePropertyAttribute keyValueAttribute:
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

                case TablePropertyAttribute tableAttribute:
                    return CreateTableAlertProperty(propertyValue, propertyName, displayName, tableAttribute);

                default:
                    throw new InvalidEnumArgumentException($"Unable to handle presentation attribute of type {presentationAttribute.GetType().Name}");
            }
        }

        /// <summary>
        /// Create a new instance of a <see cref="TableAlertProperty{T}"/> based on the given values.
        /// </summary>
        /// <param name="propertyValue">The property value, this must be an instance of <see cref="IList{T}"/>.</param>
        /// <param name="propertyName">The table property name.</param>
        /// <param name="displayName">The table property display name.</param>
        /// <param name="tableAttribute">The attribute applied to the table property.</param>
        /// <returns>The newly created <see cref="TableAlertProperty{T}"/> instance.</returns>
        private static DisplayableAlertProperty CreateTableAlertProperty(
            object propertyValue,
            string propertyName,
            string displayName,
            TablePropertyAttribute tableAttribute)
        {
            // Validate we have a proper value
            if (!(propertyValue is IList tablePropertyValue))
            {
                throw new ArgumentException("An AlertPresentationTableAttribute can only be applied to properties of type IList");
            }

            Type tableRowType = GetGenericListType(propertyValue.GetType());
            if (tableRowType == null)
            {
                throw new ArgumentException("An AlertPresentationTableAttribute can only be applied to properties of type IList<>");
            }

            // Easy way out if we're handling a single-column table
            if (tableAttribute is SingleColumnTablePropertyAttribute)
            {
                Type tablePropertyType = typeof(TableAlertProperty<>).MakeGenericType(tableRowType);
                return (DisplayableAlertProperty)Activator.CreateInstance(
                    tablePropertyType,
                    propertyName,
                    displayName,
                    tableAttribute.Order,
                    tableAttribute.ShowHeaders,
                    propertyValue);
            }

            return CreateMultiColumnTableAlertProperty(tablePropertyValue, propertyName, displayName, tableRowType, tableAttribute);
        }

        /// <summary>
        /// Create a new instance of a multi-columned <see cref="TableAlertProperty{T}"/> based on the given values.
        /// </summary>
        /// <param name="tableRows">The values of the table rows.</param>
        /// <param name="tablePropertyName">The table property name.</param>
        /// <param name="tableDisplayName">The table property display name.</param>
        /// <param name="tableRowType">The type of the table's rows.</param>
        /// <param name="tableAttribute">The attribute applied to the table property.</param>
        /// <returns>The newly created <see cref="TableAlertProperty{T}"/> instance.</returns>
        private static TableAlertProperty<Dictionary<string, string>> CreateMultiColumnTableAlertProperty(
            IList tableRows,
            string tablePropertyName,
            string tableDisplayName,
            Type tableRowType,
            TablePropertyAttribute tableAttribute)
        {
            var columns = new List<TableColumn>();
            var rows = new List<Dictionary<string, string>>(tableRows.Count);

            // Initialize the table rows with new dictionaries
            for (int i = 0; i < tableRows.Count; i++)
            {
                rows.Add(new Dictionary<string, string>());
            }

            // We scan the table by columns to we'll handle a single property at a time
            foreach (PropertyInfo columnProperty in tableRowType.GetProperties())
            {
                // Handle only table column properties
                TableColumnAttribute tableColumnAttribute = columnProperty.GetCustomAttribute<TableColumnAttribute>();
                if (tableColumnAttribute != null)
                {
                    for (int i = 0; i < tableRows.Count; i++)
                    {
                        rows[i][columnProperty.Name] = PropertyValueToString(
                            tableRows[i],
                            columnProperty,
                            columnProperty.GetValue(tableRows[i]));
                    }

                    columns.Add(new TableColumn(columnProperty.Name, tableColumnAttribute.DisplayName));
                }
            }

            return new TableAlertProperty<Dictionary<string, string>>(tablePropertyName, tableDisplayName, tableAttribute.Order, tableAttribute.ShowHeaders, columns, rows);
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
        /// Converts a presentation property's value to a string.
        /// </summary>
        /// <param name="propertyOwner">The object containing the property.</param>
        /// <param name="propertyInfo">The property's info.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns>The string</returns>
        private static string PropertyValueToString(object propertyOwner, PropertyInfo propertyInfo, object propertyValue)
        {
            if (propertyValue == null)
            {
                // null is a null string
                return null;
            }

            // Check if there's a formatter attribute on the property
            UrlFormatterAttribute uriFormatterAttribute = propertyInfo.GetCustomAttribute<UrlFormatterAttribute>();
            if (uriFormatterAttribute != null)
            {
                if (!(propertyValue is Uri uriValue))
                {
                    throw new ArgumentException("An AlertPresentationUrlFormatterAttribute can only be applied to properties of type Uri");
                }

                if (!uriValue.IsAbsoluteUri)
                {
                    throw new ArgumentException("The URI supplied must be absolute");
                }

                string linkText = uriFormatterAttribute.LinkText.EvaluateInterpolatedString(propertyOwner);
                return $"<a href=\"{uriValue}\" target=\"_blank\">{linkText}</a>";
            }

            // Otherwise - fall back to the regular conversion
            if (propertyValue is DateTime dateProperty)
            {
                // Convert to universal sortable time
                return dateProperty.ToString("u", CultureInfo.InvariantCulture);
            }
            else
            {
                return propertyValue.ToString();
            }
        }

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
                case ChartAxisType.PercentageAxis:
                    return ContractsChartAxisType.Percentage;
                default:
                    throw new InvalidEnumArgumentException($"Unsupported chart axis of type {chartAxisType}");
            }
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