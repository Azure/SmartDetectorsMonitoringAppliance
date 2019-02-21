//-----------------------------------------------------------------------
// <copyright file="MetricChartPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    /// <summary>
    /// An attribute defining the presentation of a metric chart property in an <see cref="Microsoft.Azure.Monitoring.SmartDetectors.Alert"/>.
    /// Usage of this attribute is preferable to <see cref="ChartPropertyAttribute"/> in cases where the chart displays metric data from an
    /// Azure resource, as it will ensure that the alert's data will be only accessible to users that have the appropriate read permissions.
    /// </summary>
    public class MetricChartPropertyAttribute : AlertPresentationPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricChartPropertyAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name to use when presenting the property's value.</param>
        public MetricChartPropertyAttribute(string displayName)
            : base(displayName)
        {
        }
    }
}
