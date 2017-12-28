//-----------------------------------------------------------------------
// <copyright file="SmartSignalResultPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for publishing Smart Signal results.
    /// </summary>
    public class SmartSignalResultPublisher : ISmartSignalResultPublisher
    {
        private const string ResultEventName = "SmartSignalResult";

        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResultPublisher"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        public SmartSignalResultPublisher(ITracer tracer)
        {
            this.tracer = tracer;
        }

        /// <summary>
        /// Publish Smart Signal result as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="smartSignalResult">The Smart Signal result to publish</param>
        public void PublishSignalResult(string signalId, SmartSignalResult smartSignalResult)
        {
            if (!smartSignalResult.ResultItems.Any())
            {
                this.tracer.TraceInformation("no result items to publish");
                return;
            }

            foreach (var resultItem in smartSignalResult.ResultItems)
            {
                var eventProperties = new Dictionary<string, string>
                {
                    { "SignalId", signalId },
                    { "ResultItem", JsonConvert.SerializeObject(resultItem) }
                };

                this.tracer.TrackEvent(ResultEventName, eventProperties);
            }

            this.tracer.TraceInformation($"{smartSignalResult.ResultItems.Count} Smart Signal result items were published to the results store");
        }
    }
}
