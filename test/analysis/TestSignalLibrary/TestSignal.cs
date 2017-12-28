//-----------------------------------------------------------------------
// <copyright file="TestSignal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSignalLibrary
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;

    public class TestSignal : ISmartSignal
    {
        public Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SmartSignalResult
            {
                ResultItems = new List<SmartSignalResultItem>
                {
                    new TestSignalResultItem("test title", analysisRequest.TargetResources.First())
                }
            });
        }
    }
}
