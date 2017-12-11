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

        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectionPublisher"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        public DetectionPublisher(ITracer tracer)
        {
            this.tracer = tracer;
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
                this.tracer.TraceInformation("no detections to publish");
                return;
            }

            foreach (var detection in detections)
            {
                var detectionProperties = new Dictionary<string, string>
                {
                    { "SignalId", signalId },
                    { "Detection", JsonConvert.SerializeObject(detection) }
                };

                this.tracer.TrackEvent(DetectionEventName, detectionProperties);
            }

            this.tracer.TraceInformation($"{detections.Count} detections were published to the detection store");
        }
    }
}
