namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Documents;

    public class SampleSignal : ISmartSignal

    {
        public  async Task<List<SmartSignalDetection>> AnalyzeResourcesAsync(IList<ResourceIdentifier> targetResources, TimeRange analysisWindow,
            ISmartSignalAnalysisServices analysisServices, ITracer tracer, CancellationToken cancellationToken)
        {
            SampleSignalDetection sampleSignalDetection = new SampleSignalDetection();
            List<SmartSignalDetection> smartSignalDetections = new List<SmartSignalDetection> {sampleSignalDetection};
            return smartSignalDetections;
        }
    }
}
