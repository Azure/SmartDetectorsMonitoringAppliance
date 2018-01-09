//-----------------------------------------------------------------------
// <copyright file="AnalysisExecuter.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for executing signals via the analysis flow
    /// </summary>
    public class AnalysisExecuter : IAnalysisExecuter
    {
        private readonly ITracer tracer;
        private readonly string analysisUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecuter"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        public AnalysisExecuter(ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            var functionAppBaseUrl = ConfigurationReader.ReadConfig("FunctionBaseUrl", true);
            this.analysisUrl = $"{functionAppBaseUrl}/api/Analyze";
        }

        /// <summary>
        /// Executes the signal via the analysis flow
        /// </summary>
        /// <param name="signalExecutionInfo">The signal execution information</param>
        /// <param name="resourceIds">The resource IDs used by the signal</param>
        /// <returns>A list of smart signal result items</returns>
        public async Task<IList<SmartSignalResultItemPresentation>> ExecuteSignalAsync(SignalExecutionInfo signalExecutionInfo, IList<string> resourceIds)
        {
            var analysisRequest = new SmartSignalRequest(resourceIds, signalExecutionInfo.SignalId, signalExecutionInfo.LastExecutionTime, signalExecutionInfo.Cadence, null);
            return await this.SendToAnalysisAsync(analysisRequest);
        }

        /// <summary>
        /// Sends an HTTP request to the analysis function with the smart signal request
        /// </summary>
        /// <param name="analysisRequest">The request to send to the analysis function</param>
        /// <returns>A list of smart signal result items</returns>
        private async Task<IList<SmartSignalResultItemPresentation>> SendToAnalysisAsync(SmartSignalRequest analysisRequest)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, this.analysisUrl);

            //// TODO: should add headers?
            //// requestMessage.Headers.Add("key", "value");

            string requestBody = JsonConvert.SerializeObject(analysisRequest);
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            this.tracer.TraceVerbose($"Sending analysis request {requestBody}");

            // Send the request
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var message = $"Failed to execute signal {analysisRequest.SignalId}. Fail StatusCode: {response.StatusCode}.";
                    throw new InvalidOperationException(message);
                }

                var httpAnalysisResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<SmartSignalResultItemPresentation>>(httpAnalysisResult);
            }
        }
    }
}
