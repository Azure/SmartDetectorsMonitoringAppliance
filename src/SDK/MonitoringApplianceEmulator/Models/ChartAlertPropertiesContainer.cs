//-----------------------------------------------------------------------
// <copyright file="ChartAlertPropertiesContainer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.AlertProperties;

    /// <summary>
    /// Container for multiple <see cref="ChartAlertProperty"/>.
    /// </summary>
    public class ChartAlertPropertiesContainer : DisplayableAlertProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartAlertPropertiesContainer"/> class.
        /// </summary>
        /// <param name="chartsAlertProperties">The chart alerts properties</param>
        /// <param name="order">The property order</param>
        public ChartAlertPropertiesContainer(List<ChartAlertProperty> chartsAlertProperties, byte order)
            : base(AlertPropertyType.Chart, "ChartAlertPropertiesContainer", "ChartAlertPropertiesContainer", order)
        {
            this.ChartsAlertProperties = chartsAlertProperties;
        }

        /// <summary>
        /// Gets the alert property type.
        /// </summary>
        public List<ChartAlertProperty> ChartsAlertProperties { get; }
    }
}
