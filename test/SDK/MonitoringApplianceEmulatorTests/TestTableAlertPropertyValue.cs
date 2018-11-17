//-----------------------------------------------------------------------
// <copyright file="TestTableAlertPropertyValue.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests
{
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;

    /// <summary>
    /// Illustrate a class that represents a row of a <see cref="TableAlertProperty{T}"/> for test purpose.
    /// </summary>
    public class TestTableAlertPropertyValue
    {
        [AlertPresentationTableColumn("First Name")]
        public string FirstName { get; set; }

        [AlertPresentationTableColumn("Last Name")]
        public string LastName { get; set; }

        [AlertPresentationTableColumn("Goals avg")]
        public double Goals { get; set; }
    }
}