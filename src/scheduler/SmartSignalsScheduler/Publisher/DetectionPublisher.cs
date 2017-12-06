namespace Microsoft.SmartSignals.Scheduler.Publisher
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for publishing detections.
    /// </summary>
    public class DetectionPublisher : IDetectionPublisher
    {
        private const string DetectionEventName = "Detection";

        private readonly ITracer _tracer;

        public DetectionPublisher(ITracer tracer)
        {
            _tracer = tracer;
        }

        /// <summary>
        /// Publish detections as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="detections">The detections to publish</param>
        public void PublishDetections(string signalId, IList<SmartSignalDetection> detections)
        {
            if (!detections.Any())
            {
                _tracer.TraceInformation("no detections to publish");
                return;
            }

            foreach (var detection in detections)
            {
                var detectionProperties = new Dictionary<string, string>
                {
                    {"SignalId", signalId},
                    {"Detection", JsonConvert.SerializeObject(detection)}
                };

                _tracer.TrackEvent(DetectionEventName, detectionProperties);
            }

            _tracer.TraceInformation($"{detections.Count} detections were published to the detection store");
        }
    }
}
