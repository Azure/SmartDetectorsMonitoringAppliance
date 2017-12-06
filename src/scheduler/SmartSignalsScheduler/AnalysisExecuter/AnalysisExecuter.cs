namespace Microsoft.SmartSignals.Scheduler.AnalysisExecuter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        //TODO: real analysis URL from configuration
        private string _analysisUrl = "";

        /// <summary>
        /// Execute the signal via the analysis flow
        /// </summary>
        /// <param name="signalConfiguration">The signal configuration</param>
        /// <param name="resourceIds">The resources IDs used by the signal</param>
        /// <param name="lastAnalysisEndTime">the analysis end time of the last successful run of the signal</param>
        /// <returns>The signal detections</returns>
        public async Task<SmartSignalDetection> ExecuteSignalAsync(SmartSignalConfiguration signalConfiguration, IList<string> resourceIds, DateTime? lastAnalysisEndTime = null)
        {
            // Get the window size and analysis timestamp based on the predefined CRON schedule and the last analysis time
            DateTime currentRunOccurrence;
            DateTime previousRunOccurrence;
            if (lastAnalysisEndTime == null)
            {
                // First time run - We want to get the latest possible run time based on the CRON schedule; e.g. for CRON that runs every round hour, if UtcNow is 03:07 then next run should be 03:00.
                // Since we don't have a the last successful run we take a time from distant past and from all possible execution we take the last one.
                var distantPast = DateTime.UtcNow.AddMonths(-1);
                currentRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences(distantPast, DateTime.UtcNow).Last();
                previousRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences(distantPast, currentRunOccurrence).Last();
            }
            else
            {
                // We want to get the latest possible run time based on the CRON schedule; e.g. for CRON that runs every round hour, if UtcNow is 03:07 then next run should be 03:00.
                // We take the last successful run and from that time we get all possible execution and choose the last possible one.
                currentRunOccurrence = signalConfiguration.Schedule.GetNextOccurrences((DateTime)lastAnalysisEndTime, DateTime.UtcNow).Last();
                var possiblePreviousOccurrences = signalConfiguration.Schedule.GetNextOccurrences((DateTime)lastAnalysisEndTime, currentRunOccurrence).ToList();
                previousRunOccurrence = possiblePreviousOccurrences.Any() ? possiblePreviousOccurrences.Last() : (DateTime)lastAnalysisEndTime;
            }

            var analysisRequest = new SmartSignalRequest(resourceIds, signalConfiguration.SignalId, previousRunOccurrence, currentRunOccurrence, null);
            return await SendToAnalysisAsync(analysisRequest);
        }

        private async Task<SmartSignalDetection> SendToAnalysisAsync(SmartSignalRequest analysisRequest)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _analysisUrl);

            // TODO: should add headers?
            // requestMessage.Headers.Add("key", "value");

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
                return JsonConvert.DeserializeObject<SmartSignalDetection>(httpAnalysisResult);
            }
        }
    }
}
