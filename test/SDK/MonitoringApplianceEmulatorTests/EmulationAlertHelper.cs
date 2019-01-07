//-----------------------------------------------------------------------
// <copyright file="EmulationAlertHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts.Alert;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    public static class EmulationAlertHelper
    {
        private static string appInsightsResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/microsoft.insights/components/someApp";

        private static string virtualMachineResourceId = "/subscriptions/7904b7bd-5e6b-4415-99a8-355657b7da19/resourceGroups/MyResourceGroupName/providers/Microsoft.Compute/virtualMachines/MyVirtualMachineName";

        /// <summary>
        /// Creating a new instance of <see cref="EmulationAlert"/> for unit tests.
        /// </summary>
        /// <param name="alert">The alert to wrap</param>
        /// <returns>An emulation alert</returns>
        public static EmulationAlert CreateEmulationAlert(Alert alert)
        {
            string resourceId = alert.ResourceIdentifier.ResourceType == ResourceType.ApplicationInsights ?
                appInsightsResourceId :
                virtualMachineResourceId;

            var request = new SmartDetectorAnalysisRequest
            {
                ResourceIds = new List<string> { resourceId },
                SmartDetectorId = "smartDetectorId",
                Cadence = TimeSpan.FromDays(1),
            };

            ContractsAlert contractsAlert = alert.CreateContractsAlert(request, "smartDetectorName", usedLogAnalysisClient: false, usedMetricClient: false);

            return new EmulationAlert(contractsAlert, ExtendedDateTime.UtcNow);
        }
    }
}
