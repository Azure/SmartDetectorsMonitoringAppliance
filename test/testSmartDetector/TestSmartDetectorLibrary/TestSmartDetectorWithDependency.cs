//-----------------------------------------------------------------------
// <copyright file="TestSmartDetectorWithDependency.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using TestSmartDetectorDependentLibrary;

    public class TestSmartDetectorWithDependency : ISmartDetector
    {
        public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            int[] obj = { 1, 2, 3 };
            var dependent = new DependentClass();
            List<Alert> alerts = new List<Alert>();
            alerts.Add(new TestAlert(
                "test title - " + dependent.GetString() + " - " + dependent.ObjectToString(obj),
                analysisRequest.TargetResources.First(),
                AlertState.Active));
            return Task.FromResult(alerts);
        }
    }
}