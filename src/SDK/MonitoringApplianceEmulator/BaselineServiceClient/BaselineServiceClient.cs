//-----------------------------------------------------------------------
// <copyright file="BaselineServiceClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable CA1724

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Polly;

    /// <summary>
    /// An implementation of the <see cref="IBaselineServiceClient"/> interface
    /// </summary>
    public class BaselineServiceClient : IBaselineServiceClient
    {
        private const int RetryCount = 5;

        private static readonly string BaselineServiceUrlFormat = ConfigurationManager.AppSettings["BaselineServiceUriPattern"] ?? throw new ApplicationException("Could not find setting BaselineServiceUriPattern");

        private readonly HttpClient httpClient;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaselineServiceClient"/> class.
        /// </summary>
        /// <param name="httpClient">The http client to use</param>
        public BaselineServiceClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            this.retryPolicy = Policy
                .Handle<BaselineServiceRequestFailedException>(e => e.ResponseStatusCode != null && e.ResponseStatusCode.Value >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(RetryCount, (i) => TimeSpan.FromSeconds(Math.Pow(2, i)));
        }

        #region Implementation of IBaselineServiceClient

        /// <summary>
        /// Sends an IsAnomaly request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The IsAnomaly response</returns>
        public async Task<IsAnomalyResponseDto> IsAnomalyAsync(IsAnomalyRequestDto requestDto, CancellationToken cancellationToken)
        {
            return await this.retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var stringContent = GetJsonStringContent(requestDto);
                    var responseMessage = await this.httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, string.Format(CultureInfo.InvariantCulture, BaselineServiceUrlFormat, "anomaly", "api/IsAnomaly/Arm"))
                        {
                            Content = stringContent
                        },
                        cancellationToken).ConfigureAwait(false);

                    var responseBody = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        if (responseMessage.StatusCode == HttpStatusCode.PreconditionFailed)
                        {
                            throw new BaselineServiceModelMissingException("Baseline model is missing");
                        }
                        else if (responseMessage.StatusCode == HttpStatusCode.Gone)
                        {
                            throw new BaselineServiceModelUntrainedException("Baseline model is not trained");
                        }
                        else
                        {
                            var errorMsg = $"Failed to get anomaly score, code {responseMessage.StatusCode}";
                            throw new BaselineServiceRequestFailedException(errorMsg, responseMessage.StatusCode);
                        }
                    }

                    var serviceResponse = JsonConvert.DeserializeObject<IsAnomalyResponseDto>(responseBody);
                    if (serviceResponse.AnomalyEvaluationResult == null)
                    {
                        throw new BaselineServiceRequestFailedException(
                            "Failed to get anomaly score, result is null",
                            null);
                    }

                    return serviceResponse;
                }
                catch (BaselineServiceException)
                {
                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    throw new BaselineServiceTimeoutException("Failed to get anomaly score due to timeout", ex);
                }
                catch (Exception ex)
                {
                    throw new BaselineServiceIsAnomalyException("Failed to get anomaly score", ex);
                }
            });
        }

        /// <summary>
        /// Sends a GetQueriesForTraining request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The GetQueriesForTraining response</returns>
        public async Task<GetQueriesForTrainResponseDto> GetQueriesForTrainingAsync(GetQueriesForTrainRequestDto requestDto, CancellationToken cancellationToken)
        {
            return await this.retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var stringContent = GetJsonStringContent(requestDto);
                    var responseMessage = await this.httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, string.Format(CultureInfo.InvariantCulture, BaselineServiceUrlFormat, "train", "api/GetQueriesForTrain/Arm"))
                        {
                            Content = stringContent
                        },
                        cancellationToken).ConfigureAwait(false);

                    var responseBody = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        throw new BaselineServiceRequestFailedException(
                            $"Failed to get training queries {responseMessage.StatusCode}: {responseBody}",
                            responseMessage.StatusCode);
                    }

                    return JsonConvert.DeserializeObject<GetQueriesForTrainResponseDto>(responseBody);
                }
                catch (BaselineServiceException)
                {
                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    throw new BaselineServiceGetQueriesTimeoutException(
                        "Failed to retrieve training queries due to timeout", ex);
                }
                catch (Exception ex)
                {
                    throw new BaselineServiceGetQueriesException($"Failed to retrieve training queries: {ex.Message}", null);
                }
            });
        }

        /// <summary>
        /// Send a Train request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Train response</returns>
        public async Task TrainAsync(TrainRequestDto requestDto, CancellationToken cancellationToken)
        {
            await this.retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var stringContent = GetJsonStringContent(requestDto);
                    var responseMessage = await this.httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, string.Format(CultureInfo.InvariantCulture, BaselineServiceUrlFormat, "train", "api/Train/Arm"))
                        {
                            Content = stringContent
                        },
                        cancellationToken).ConfigureAwait(false);

                    var responseBody = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        throw new BaselineServiceRequestFailedException(
                            $"Failed to train data {responseMessage.StatusCode}: {responseBody}",
                            responseMessage.StatusCode);
                    }
                }
                catch (BaselineServiceException)
                {
                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    throw new BaselineServiceTrainTimeoutException("Failed to train data due to timeout", ex);
                }
                catch (Exception ex)
                {
                    throw new BaselineServiceTrainException($"Failed to train data: {ex.Message}", null);
                }
            });
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="StringContent"/> object, containing the specified payload
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="payload">The payload</param>
        /// <returns>The <see cref="StringContent"/> object, containing the payload</returns>
        private static StringContent GetJsonStringContent<T>(T payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
