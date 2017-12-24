//-----------------------------------------------------------------------
// <copyright file="TestSignalWithDependency.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using TestSignalDependentLibrary;

    public class TestSignalWithDependency : ISmartSignal
    {
        public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
        {
            int[] obj = { 1, 2, 3 };
            var dependent = new DependentClass();
            return Task.FromResult(new List<SmartSignalDetection>()
            {
                new TestSignalDetection("test title - " + dependent.GetString() + " - " + dependent.ObjectToString(obj))
            });
        }
    }
}