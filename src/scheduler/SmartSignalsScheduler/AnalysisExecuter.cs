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
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for executing signals via the analysis flow
    /// </summary>
    public class AnalysisExecuter : IAnalysisExecuter
    {
        private readonly string analysisUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecuter"/> class.
        /// </summary>
        public AnalysisExecuter()
        {
            var functionAppBaseUrl = ConfigurationReader.ReadConfig("FunctionBaseUrl", true);
            this.analysisUrl = $"{functionAppBaseUrl}/api/Analyze";
        }

        /// <summary>
        /// Executes the signal via the analysis flow
        /// </summary>
        /// <param name="signalExecutionInfo">The signal execution information</param>
        /// <param name="resourceIds">The resource IDs used by the signal</param>
        /// <returns>The signal result</returns>
        public async Task<SmartSignalResult> ExecuteSignalAsync(SignalExecutionInfo signalExecutionInfo, IList<string> resourceIds)
        {
            var analysisRequest = new SmartSignalRequest(resourceIds, signalExecutionInfo.SignalId, signalExecutionInfo.LastExecutionTime, signalExecutionInfo.Cadence, null);
            return await this.SendToAnalysisAsync(analysisRequest);
        }

        /// <summary>
        /// Sends an HTTP request to the analysis function with the smart signal request
        /// </summary>
        /// <param name="analysisRequest">The request to send to the analysis function</param>
        /// <returns>The Smart Signal result</returns>
        private async Task<SmartSignalResult> SendToAnalysisAsync(SmartSignalRequest analysisRequest)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, this.analysisUrl);

            //// TODO: should add headers?
            //// requestMessage.Headers.Add("key", "value");

            var requestBody = JsonConvert.SerializeObject(analysisRequest);
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            
            // Send the request
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var message =
                        $"Failed to execute signal {analysisRequest.SignalId}. Fail StatusCode: {response.StatusCode}. " +
                        $"Fail StatusCode: {response.StatusCode}{Environment.NewLine}";

                    throw new InvalidOperationException(message);
                }

                var httpAnalysisResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SmartSignalResult>(httpAnalysisResult);
            }
        }
    }
}
