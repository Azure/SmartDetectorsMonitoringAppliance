//-----------------------------------------------------------------------
// <copyright file="AnalysisExecuter.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Extensions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Newtonsoft.Json;
    using Polly;

    /// <summary>
    /// This class is responsible for executing signals via the analysis flow
    /// </summary>
    public class AnalysisExecuter : IAnalysisExecuter
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "Analysis";

        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ITracer tracer;
        private readonly string analysisUrl;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecuter"/> class.
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="tracer">Log wrapper</param>
        public AnalysisExecuter(IHttpClientWrapper httpClientWrapper, ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);

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
            string requestBody = JsonConvert.SerializeObject(analysisRequest);
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            this.tracer.TraceVerbose($"Sending analysis request {requestBody}");

            // Send the request
            var response = await this.retryPolicy.RunAndTrackDependencyAsync(this.tracer, DependencyName, analysisRequest.SignalId, () => this.httpClientWrapper.SendAsync(requestMessage, default(CancellationToken)));

            if (!response.IsSuccessStatusCode)
            {
                string content = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                var message = $"Failed to execute signal {analysisRequest.SignalId}. Fail StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}. Content: {content}.";
                throw new AnalysisExecutionException(message);
            }

            var httpAnalysisResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IList<SmartSignalResultItemPresentation>>(httpAnalysisResult);
        }
    }
}
