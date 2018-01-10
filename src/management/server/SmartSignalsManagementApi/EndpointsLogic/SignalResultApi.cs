//-----------------------------------------------------------------------
// <copyright file="SignalResultApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Newtonsoft.Json;

    /// <summary>
    /// This class contains the logic for the /signalResult endpoint.
    /// </summary>
    public class SignalResultApi : ISignalResultApi
    {
        private const string EventName = "SmartSignalResult";
        private readonly IApplicationInsightsClient applicationInsightsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultApi"/> class.
        /// </summary>
        /// <param name="applicationInsightsClient">The application insights client.</param>
        public SignalResultApi(IApplicationInsightsClient applicationInsightsClient)
        {
            this.applicationInsightsClient = applicationInsightsClient;
        }

        /// <summary>
        /// Gets all the Smart Signals results.
        /// </summary>
        /// <param name="startTime">The query start time.</param>
        /// <param name="endTime">The query end time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Signals results response.</returns>
        public async Task<ListSmartSignalsResultsResponse> GetAllSmartSignalResultsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
        {
            try
            {
                // Get the custom events from 
                IEnumerable<ApplicationInsightsEvent> events = await this.applicationInsightsClient.GetCustomEventsAsync(EventName, startTime, endTime, cancellationToken);

                // Deserialize the smart signal part from the custom dimension
                var signalResults = events.Where(result => result.CustomDimensions.ContainsKey("ResultItem")).Select(result => result.CustomDimensions["ResultItem"]);

                // Deserialize and return
                IEnumerable<SmartSignalResultItemPresentation> smartSignalsResults = signalResults.Select(JsonConvert.DeserializeObject<SmartSignalResultItemPresentation>);

                return new ListSmartSignalsResultsResponse
                {
                    SignalsResults = smartSignalsResults.ToList()
                };
            }
            catch (ApplicationInsightsClientException e)
            {
                throw new SmartSignalsManagementApiException("Failed to query smart signals results due to an exception from Application Insights", e, HttpStatusCode.InternalServerError);
            }
            catch (JsonException e)
            {
                throw new SmartSignalsManagementApiException("Failed to de-serialize signals results items", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}
