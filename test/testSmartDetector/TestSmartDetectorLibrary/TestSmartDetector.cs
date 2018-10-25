//-----------------------------------------------------------------------
// <copyright file="TestSmartDetector.cs" company="Microsoft Corporation">
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

    public class TestSmartDetector : ISmartDetector
    {
        public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            List<Alert> alerts = new List<Alert>();
            alerts.Add(new TestAlert("test title", analysisRequest.TargetResources.First(), AlertState.Active));
            return Task.FromResult(alerts);
        }
    }
}
