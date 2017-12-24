//-----------------------------------------------------------------------
// <copyright file="TestSignal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;

    public class TestSignal : ISmartSignal
    {
        public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow, ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<SmartSignalDetection>()
            {
                new TestSignalDetection("test title")
            });
        }
    }
}
