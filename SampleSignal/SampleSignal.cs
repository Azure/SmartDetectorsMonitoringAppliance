namespace Microsoft.Azure.Monitoring.SmartSignals.SampleSignal
{
    using System.Threading;
    using System.Threading.Tasks;

    public class SampleSignal : ISmartSignal

    {

        public async Task<SmartSignalResult> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
        {
            SmartSignalResult smartSignalResult = new SmartSignalResult();
            analysisRequest.TargetResources.ForEach(resourceIdentifier => smartSignalResult.ResultItems.Add(new SampleSignalResultItem("Sample Signal title", resourceIdentifier)));
            return smartSignalResult;
        }
    }
}
