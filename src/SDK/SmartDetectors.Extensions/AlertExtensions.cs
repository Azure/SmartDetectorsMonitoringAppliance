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
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;
    using Newtonsoft.Json;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartAxisType;
    using ChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartPoint;
    using ChartType = Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation.ChartType;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ContractsChartAxisType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartAxisType;
    using ContractsChartPoint = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartPoint;
    using ContractsChartType = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties.ChartType;

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

            // Create presentation elements for each alert property
            List<AlertProperty> alertProperties = ExtractProperties(alert);

            string id = string.Join("##", alert.GetType().FullName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(alert)).ToSha256Hash();
            string resourceId = alert.ResourceIdentifier.ToResourceId();
            string correlationHash = string.Join("##", alert.ExtractPredicates().OrderBy(x => x.Key).Select(x => x.Key + "|" + x.Value.ToString())).ToSha256Hash();

            // Get the alert's signal type based on the clients used to create the alert
            SignalType signalType = GetSignalType(usedLogAnalysisClient, usedMetricClient);

            // Return the presentation object
            return new ContractsAlert
            {
                Title = alert.Title,
                OccurenceTime = alert.OccurenceTime,
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
        /// <param name="propertiesOwner">The object from which to extract the properties</param>
        /// <param name="order">The order to use</param>
        /// <param name="parentPropertyName">The parent property name</param>
        /// <returns>The extracted properties</returns>
        private static List<AlertProperty> ExtractProperties(object propertiesOwner, Order order = null, string parentPropertyName = null)
        {
            // The null object has no properties
            if (propertiesOwner == null)
            {
                return new List<AlertProperty>();
            }

            // Initialize order if needed
            if (order == null)
            {
                order = new Order();
            }

            // Collect all object properties, and sort them by order
            var orderedProperties = propertiesOwner.GetType().GetProperties().Select(property =>
                {
                    // Get the property value
                    object propertyValue = property.GetValue(propertiesOwner);

                    // Skip the property if it is empty
                    if (propertyValue == null ||
                        (propertyValue is string stringValue && string.IsNullOrWhiteSpace(stringValue)) ||
                        (propertyValue is ICollection collectionValue && collectionValue.Count == 0))
                    {
                        return null;
                    }

                    // Get the presentation attribute
                    AlertPresentationPropertyAttribute presentationAttribute = property.GetCustomAttribute<AlertPresentationPropertyAttribute>();

                    // Return the presentation attribute, property, and value
                    return new { PresentationAttribute = presentationAttribute, Property = property, Value = propertyValue };
                })
                .Where(x => x != null)
                .OrderBy(p => p.PresentationAttribute?.Order ?? -1)
                .ThenBy(p => p.Property.Name);

            // Process the properties, in order
            List<AlertProperty> alertProperties = new List<AlertProperty>();
            foreach (var p in orderedProperties)
            {
                if (p.PresentationAttribute != null)
                {
                    alertProperties.AddRange(CreateAlertProperties(propertiesOwner, p.Property, p.PresentationAttribute, p.Value, order, parentPropertyName));
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
        /// Creates one or more <see cref="AlertProperty"/> objects based on an alert presentation property
        /// </summary>
        /// <param name="propertyOwner">The object that has the property</param>
        /// <param name="property">The property info of the property to create</param>
        /// <param name="presentationAttribute">The attribute defining the presentation of the alert property</param>
        /// <param name="propertyValue">The property value</param>
        /// <param name="order">The order to use</param>
        /// <param name="parentPropertyName">The parent property name</param>
        /// <returns>The <see cref="AlertProperty"/> objects</returns>
        private static IEnumerable<AlertProperty> CreateAlertProperties(object propertyOwner, PropertyInfo property, AlertPresentationPropertyAttribute presentationAttribute, object propertyValue, Order order, string parentPropertyName)
        {
            // Get the attribute display name
            string displayName = presentationAttribute.DisplayName.EvaluateInterpolatedString(propertyOwner);

            // Get the property name (and add the parent property name as prefix, if provided)
            string propertyName = string.IsNullOrWhiteSpace(presentationAttribute.PropertyName) ? property.Name : presentationAttribute.PropertyName;
            if (!string.IsNullOrWhiteSpace(parentPropertyName))
            {
                propertyName = $"{parentPropertyName}_{propertyName}";
            }

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
                    yield return new LongTextAlertProprety(propertyName, displayName, order.Next(), PropertyValueToString(propertyOwner, property, propertyValue));
                    break;

                case TextPropertyAttribute _:
                    yield return new TextAlertProperty(propertyName, displayName, order.Next(), PropertyValueToString(propertyOwner, property, propertyValue));
                    break;

                case ListPropertyAttribute _:
                    if (!(propertyValue is IList list))
                    {
                        throw new ArgumentException("A ListPropertyAttribute can only be applied to properties of type IList");
                    }

                    foreach (AlertProperty p in CreateAlertPropertiesFromList(propertyName, list, order))
                    {
                        yield return p;
                    }

                    break;

                case KeyValuePropertyAttribute keyValueAttribute:
                    if (!(propertyValue is IDictionary<string, string> keyValuePropertyValue))
                    {
                        throw new ArgumentException("An AlertPresentationKeyValueAttribute can only be applied to properties of type IDictionary<string, string>");
                    }

                    if (keyValueAttribute.ShowHeaders)
                    {
                        string keyHeaderName = keyValueAttribute.KeyHeaderName.EvaluateInterpolatedString(propertyOwner);
                        string valueHeaderName = keyValueAttribute.ValueHeaderName.EvaluateInterpolatedString(propertyOwner);
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
        /// <param name="order">The order to use</param>
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

            // Validate the table is not empty (shouldn't happen, empty tables are ignored by ExtractProperties)
            if (tablePropertyValue.Count == 0)
            {
                throw new ArgumentException("Unexpected empty list encountered");
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
        /// <param name="order">The order to use</param>
        /// <returns>The newly created <see cref="TableAlertProperty{T}"/> instance.</returns>
        private static TableAlertProperty<Dictionary<string, string>> CreateMultiColumnTableAlertProperty(
            IList tableRows,
            string tablePropertyName,
            string tableDisplayName,
            Type tableRowType,
            TablePropertyAttribute tableAttribute,
            Order order)
        {
            var rows = new List<Dictionary<string, string>>(tableRows.Count);

            // Initialize the table rows with new dictionaries
            for (int i = 0; i < tableRows.Count; i++)
            {
                rows.Add(new Dictionary<string, string>());
            }

            // We scan the table by columns to we'll handle a single property at a time
            var columnsWithOrder = new List<Tuple<TableColumn, byte>>();
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

                    // Store the column and its order
                    columnsWithOrder.Add(Tuple.Create(new TableColumn(columnProperty.Name, tableColumnAttribute.DisplayName), tableColumnAttribute.Order));
                }
            }

            // Get the columns in order
            List<TableColumn> columns = columnsWithOrder.OrderBy(t => t.Item2).Select(t => t.Item1).ToList();

            return new TableAlertProperty<Dictionary<string, string>>(tablePropertyName, tableDisplayName, order.Next(), tableAttribute.ShowHeaders, columns, rows);
        }

        /// <summary>
        /// Create a list of properties, extracted from the objects in the specified list.
        /// </summary>
        /// <param name="listPropertyName">The list property name</param>
        /// <param name="list">The list of objects, from which to extract the properties</param>
        /// <param name="order">The order to use</param>
        /// <returns>The newly created properties.</returns>
        private static List<AlertProperty> CreateAlertPropertiesFromList(string listPropertyName, IList list, Order order)
        {
            List<AlertProperty> alertProperties = new List<AlertProperty>();
            for (int i = 0; i < list.Count; i++)
            {
                List<AlertProperty> objectProperties = ExtractProperties(list[i], order, $"{listPropertyName}_{i}");
                alertProperties.AddRange(objectProperties);
            }

            return alertProperties;
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
        /// A helper class to keep track of the order of presentation properties
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