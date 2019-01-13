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
        private static readonly HashSet<string> AlertBaseClassPropertiesNames = new HashSet<string>(typeof(Alert).GetProperties().Select(p => p.Name));

        /// <summary>
        /// Creates a presentation from an alert
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="request">The Smart Detector request</param>
        /// <param name="smartDetectorName">The Smart Detector name</param>
        /// <param name="usedLogAnalysisClient">Indicates whether a log analysis client was used to create the alert</param>
        /// <param name="usedMetricClient">Indicates whether a metric client was used to create the alert</param>
        /// <returns>The presentation</returns>
        public static ContractsAlert CreateContractsAlert(this Alert alert, SmartDetectorAnalysisRequest request, string smartDetectorName, bool usedLogAnalysisClient, bool usedMetricClient)
        {
            // A null alert has null presentation
            if (alert == null)
            {
                return null;
            }

            // Extract the alert properties
            List<AlertProperty> alertProperties = ExtractProperties(alert, new Order());

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).ToSha256Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", alert.ExtractPredicates().OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value.ToString())).ToSha256Hash();

            // Get the alert's signal type based on the clients used to create the alert
            SignalType signalType = GetSignalType(usedLogAnalysisClient, usedMetricClient);

            // Return the presentation object
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
                AlertProperties = alertProperties,
                SignalType = signalType,
                ResolutionParameters = alert.AlertResolutionParameters?.CreateContractsResolutionParameters()
            };
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
        /// Extract all alert properties from the specified object
        /// </summary>
        /// <param name="alert">The object from which to extract the properties</param>
        /// <param name="order">The order to use</param>
        /// <returns>The extracted properties</returns>
        private static List<AlertProperty> ExtractProperties(object alert, Order order)
        {
            // Collect all object properties, and sort them by order
            var propertyDetails = alert.GetType().GetProperties().Select(property =>
                {
                    // Get the property value
                    object propertyValue = property.GetValue(alert);
                    string propertyStringValue = PropertyValueToString(alert, property, propertyValue);
                    if (string.IsNullOrWhiteSpace(propertyStringValue) || (propertyValue is ICollection value && value.Count == 0))
                    {
                        // skip empty properties
                        return null;
                    }

                    // Get the presentation attribute
                    AlertPresentationPropertyAttribute presentationAttribute = property.GetCustomAttribute<AlertPresentationPropertyAttribute>();

                    return new { PresentationAttribute = presentationAttribute, Property = property, Value = propertyValue };
                })
                .Where(x => x != null)
                .OrderBy(p => p.PresentationAttribute?.Order ?? -1)
                .ThenBy(p => p.Property.Name)
                .ToList();

            // Go over the properties, in order
            List<AlertProperty> alertProperties = new List<AlertProperty>();
            foreach (var p in propertyDetails)
            {
                if (p.PresentationAttribute != null)
                {
                    alertProperties.AddRange(CreateAlertProperty(alert, p.Property, p.PresentationAttribute, p.Value, order));
                }
                else if (!AlertBaseClassPropertiesNames.Contains(p.Property.Name))
                {
                    // Get the raw alert property - a property with no presentation
                    alertProperties.Add(new RawAlertProperty(p.Property.Name, p.Value));
                }
            }

            return alertProperties;
        }

        /// <summary>
        /// Creates an <see cref="AlertProperty"/> based on an alert presentation V2 property
        /// </summary>
        /// <param name="alert">The alert</param>
        /// <param name="property">The property info of the property to create</param>
        /// <param name="presentationAttribute">The attribute defining the presentation V2 of the alert property</param>
        /// <param name="propertyValue">The property value</param>
        /// <param name="order">The current order</param>
        /// <returns>An <see cref="AlertProperty"/></returns>
        private static IEnumerable<AlertProperty> CreateAlertProperty(object alert, PropertyInfo property, AlertPresentationPropertyAttribute presentationAttribute, object propertyValue, Order order)
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

                    yield return new ChartAlertProperty(
                        propertyName,
                        displayName,
                        order.Next(),
                        ConvertChartTypeToContractsChartType(chartAttribute.ChartType),
                        ConvertChartAxisTypeToContractsChartType(chartAttribute.XAxisType),
                        ConvertChartAxisTypeToContractsChartType(chartAttribute.YAxisType),
                        listValues.Select(point => new ContractsChartPoint(point.X, point.Y)).ToList());
                    break;

                case LongTextPropertyAttribute _:
                    yield return new LongTextAlertProprety(propertyName, displayName, order.Next(), PropertyValueToString(alert, property, propertyValue));
                    break;

                case TextPropertyAttribute _:
                    if (propertyValue is IList list)
                    {
                        foreach (DisplayableAlertProperty p in CreateListOfAlertProperties(list, order))
                        {
                            yield return p;
                        }
                    }
                    else
                    {
                        yield return new TextAlertProperty(propertyName, displayName, order.Next(), PropertyValueToString(alert, property, propertyValue));
                    }

                    break;

                case KeyValuePropertyAttribute keyValueAttribute:
                    if (!(propertyValue is IDictionary<string, string> keyValuePropertyValue))
                    {
                        throw new ArgumentException("An AlertPresentationKeyValueAttribute can only be applied to properties of type IDictionary<string, string>");
                    }

                    if (keyValueAttribute.ShowHeaders)
                    {
                        string keyHeaderName = keyValueAttribute.KeyHeaderName.EvaluateInterpolatedString(alert);
                        string valueHeaderName = keyValueAttribute.ValueHeaderName.EvaluateInterpolatedString(alert);
                        yield return new KeyValueAlertProperty(propertyName, displayName, order.Next(), keyHeaderName, valueHeaderName, keyValuePropertyValue);
                    }
                    else
                    {
                        yield return new KeyValueAlertProperty(propertyName, displayName, order.Next(), keyValuePropertyValue);
                    }

                    break;

                case TablePropertyAttribute tableAttribute:
                    yield return CreateTableAlertProperty(propertyValue, propertyName, displayName, tableAttribute, order);
                    break;

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
        /// <param name="order">The current order</param>
        /// <returns>The newly created <see cref="TableAlertProperty{T}"/> instance.</returns>
        private static DisplayableAlertProperty CreateTableAlertProperty(
            object propertyValue,
            string propertyName,
            string displayName,
            TablePropertyAttribute tableAttribute,
            Order order)
        {
            // Validate we have a proper value
            if (!(propertyValue is IList tablePropertyValue))
            {
                throw new ArgumentException("An AlertPresentationTableAttribute can only be applied to properties of type IList");
            }

            // Empty table  - this should have been taken care of in ExtractProperties, but we check here also just in case
            if (tablePropertyValue.Count == 0)
            {
                throw new ArgumentException("An AlertPresentationTableAttribute cannot be applied to an empty list");
            }

            // Get element type, and verify that all elements are of the same type
            Type tableRowType = tablePropertyValue[0].GetType();
            foreach (object item in tablePropertyValue)
            {
                if (item.GetType() != tableRowType)
                {
                    throw new ArgumentException("All items in a list with AlertPresentationTableAttribute must have the same type");
                }
            }

            // Easy way out if we're handling a single-column table
            if (tableAttribute is SingleColumnTablePropertyAttribute)
            {
                Type tablePropertyType = typeof(TableAlertProperty<>).MakeGenericType(tableRowType);
                return (DisplayableAlertProperty)Activator.CreateInstance(
                    tablePropertyType,
                    propertyName,
                    displayName,
                    order.Next(),
                    tableAttribute.ShowHeaders,
                    propertyValue);
            }

            return CreateMultiColumnTableAlertProperty(tablePropertyValue, propertyName, displayName, tableRowType, tableAttribute, order);
        }

        /// <summary>
        /// Create a new instance of a multi-columned <see cref="TableAlertProperty{T}"/> based on the given values.
        /// </summary>
        /// <param name="tableRows">The values of the table rows.</param>
        /// <param name="tablePropertyName">The table property name.</param>
        /// <param name="tableDisplayName">The table property display name.</param>
        /// <param name="tableRowType">The type of the table's rows.</param>
        /// <param name="tableAttribute">The attribute applied to the table property.</param>
        /// <param name="order">The current order</param>
        /// <returns>The newly created <see cref="TableAlertProperty{T}"/> instance.</returns>
        private static TableAlertProperty<Dictionary<string, string>> CreateMultiColumnTableAlertProperty(
            IList tableRows,
            string tablePropertyName,
            string tableDisplayName,
            Type tableRowType,
            TablePropertyAttribute tableAttribute,
            Order order)
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

            return new TableAlertProperty<Dictionary<string, string>>(tablePropertyName, tableDisplayName, order.Next(), tableAttribute.ShowHeaders, columns, rows);
        }

        /// <summary>
        /// Create a list of properties, extracted from the objects in the specified list.
        /// </summary>
        /// <param name="list">The list of objects, from which to extract the properties</param>
        /// <param name="order">The current order</param>
        /// <returns>The newly created properties.</returns>
        private static List<DisplayableAlertProperty> CreateListOfAlertProperties(IList list, Order order)
        {
            List<DisplayableAlertProperty> displayableAlertProperties = new List<DisplayableAlertProperty>();
            foreach (object obj in list)
            {
                List<AlertProperty> objectProperties = ExtractProperties(obj, order);
                displayableAlertProperties.AddRange(objectProperties.OfType<DisplayableAlertProperty>());
            }

            return displayableAlertProperties;
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
        /// A helper class to keep track of the order of properties
        /// </summary>
        private class Order
        {
            private byte currentOrder;

            /// <summary>
            /// Initializes a new instance of the <see cref="Order"/> class
            /// </summary>
            public Order()
            {
                // Initialize the order to 0
                this.currentOrder = 0;
            }

            /// <summary>
            /// Get the next order
            /// </summary>
            /// <returns>The next order value</returns>
            public byte Next()
            {
                if (this.currentOrder == byte.MaxValue)
                {
                    // The alert has too many properties - ignore the order from now on
                    return this.currentOrder;
                }
                else
                {
                    // Return and increment
                    return this.currentOrder++;
                }
            }
        }
    }
}