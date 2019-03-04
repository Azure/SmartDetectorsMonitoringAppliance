//-----------------------------------------------------------------------
// <copyright file="IBaselineServiceClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The Baseline Service client, that can be used to fetch baseline of metrics
    /// </summary>
    /// <summary>
    /// An interface for baseline service clients
    /// </summary>
    public interface IBaselineServiceClient
    {
        /// <summary>
        /// Sends an IsAnomaly request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The IsAnomaly response</returns>
        Task<IsAnomalyResponseDto> IsAnomalyAsync(IsAnomalyRequestDto requestDto, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a GetQueriesForTraining request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The GetQueriesForTraining response</returns>
        Task<GetQueriesForTrainResponseDto> GetQueriesForTrainingAsync(GetQueriesForTrainRequestDto requestDto, CancellationToken cancellationToken);

        /// <summary>
        /// Send a Train request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Train response</returns>
        Task TrainAsync(TrainRequestDto requestDto, CancellationToken cancellationToken);

        /// <summary>
        /// Send a GetPredictions request to the service
        /// </summary>
        /// <param name="requestDto">The request DTO</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The GetPredictions response</returns>
        Task<GetPredictionsResponseDto> GetPredictionsAsync(GetPredictionsRequestDto requestDto, CancellationToken cancellationToken);
    }
}