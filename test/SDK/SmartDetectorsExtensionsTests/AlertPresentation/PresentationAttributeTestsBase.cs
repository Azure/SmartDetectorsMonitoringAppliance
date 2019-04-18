//-----------------------------------------------------------------------
// <copyright file="PresentationAttributeTestsBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;

    /// <summary>
    /// Base class for testing alert property presentation attributes
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1052:StaticHolderTypesShouldBeSealed", Justification = "Test code, approved")]
    public abstract class PresentationAttributeTestsBase
    {
        private static readonly SmartDetectorAnalysisRequest AnalysisRequest = new SmartDetectorAnalysisRequest
        {
            ResourceIds = new List<string> { "resourceId" },
            SmartDetectorId = "smartDetectorId",
            Cadence = TimeSpan.FromDays(1),
        };

        protected static ContractsAlert CreateContractsAlert<TAlert>()
            where TAlert : TestAlertBase, new()
        {
            return new TAlert().CreateContractsAlert(AnalysisRequest, "detector", false, false);
        }

        public abstract class TestAlertBase : Alert
        {
            protected TestAlertBase()
                : base("AlertTitle", default(ResourceIdentifier))
            {
            }
        }
    }
}
