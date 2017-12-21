namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SampleSignal : ISmartSignal

    {
        public Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow,
            ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
