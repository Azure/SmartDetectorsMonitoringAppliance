﻿//-----------------------------------------------------------------------
// <copyright file="BaselineServiceDtos.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable CS1591, SA1649, SA1600, SA1402, SA1602, CA2227, SA1201

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class TrainRequestDto
    {
        public ArmMetricMetadata ArmMetricMetadata { get; set; }

        public TrainConfig TrainConfig { get; set; }
    }

    public class TrainConfig
    {
        public List<BaselineTimeSeries> DataList { get; set; }

        public bool ResetHistory { get; set; }

        public TimeSpan AdjustedTimespan { get; set; }

        public bool? SyncMode { get; set; }

        public string MonitorName { get; set; }

        public string ContextId { get; set; }

        public DateTime? GlobalStartTime { get; set; }
    }

    public class BaselineTimeSeries
    {
        public DataRange DataRange { get; set; }

        public List<double?> Values { get; set; }
    }

    public class GetQueriesForTrainRequestDto
    {
        public GetQueriesForTrainConfig GetQueriesForTrainConfig { get; set; }

        public ArmMetricMetadata ArmMetricMetadata { get; set; }
    }

    public class GetQueriesForTrainConfig
    {
        public DateTime? SimulatedNow { get; set; }
    }

    public class GetQueriesForTrainResponseDto
    {
        public List<MetricQueryMetadata> QueryMetadataList { get; set; }
    }

    public class MetricQueryMetadata
    {
        public List<DataRange> DataRanges { get; set; }

        public bool ResetHistory { get; set; }

        public TimeSpan AdjustedBinSize { get; set; }

        public string ContextId { get; set; }

        public override string ToString()
        {
            return $"{nameof(this.DataRanges)}: {string.Join(",", this.DataRanges.Select(dr => $"{dr.StartTime:o}:{dr.EndTime:o}"))}," +
                   $"{nameof(this.ResetHistory)}: {this.ResetHistory}, {nameof(this.AdjustedBinSize)}: {this.AdjustedBinSize}, {nameof(this.ContextId)}: {this.ContextId}";
        }
    }

    public class DataRange
    {
        public DataRange(DateTime startTime, DateTime endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            return $"DataRange: {this.StartTime} <-> {this.EndTime}";
        }
    }

    public class IsAnomalyConfig
    {
        public DateTime DataEndTime { get; set; }

        [JsonProperty(PropertyName = "CurrentLookbackPeriodPoints")]
        public IList<double?> EvaluationPointValues { get; set; }

        public string Sensitivity { get; set; }

        public ThresholdDirection ThresholdDirection { get; set; }

        [JsonProperty(PropertyName = "AnomalyResultType")]
        public int? AnomalyEvaluationOptions { get; set; }

        public string AnomalyEvaluationData { get; set; }

        public AnomalyEvaluationType? AnomalyEvaluationType { get; set; }
    }

    public class IsAnomalyRequestDto
    {
        public IsAnomalyConfig IsAnomalyConfig { get; set; }

        public ArmMetricMetadata ArmMetricMetadata { get; set; }
    }

    public class IsAnomalyResponseDto
    {
        public double? AnomalyEvaluationResult { get; set; }

        public IList<double> HighThresholdValues { get; set; }

        public IList<double> LowThresholdValues { get; set; }

        public string ViolatedThresholdValue { get; set; }

        public string ViolatedThresholdDirection { get; set; }
    }

    public enum ThresholdDirection
    {
        Up = 0,
        Down = 1,
        UpAndDown = 2,
    }

    /// <summary>
    /// Specifies the evaluation types currently supported the by baseline service.
    /// <remarks>The evaluation type is forwarded to the baseline service as an integer, and the values of this enum are aligned with those the baseline service uses and apply it's
    /// logic accordingly</remarks>
    /// </summary>
    public enum AnomalyEvaluationType
    {
        /// <summary>
        /// Consider as anomaly if all points deviates from threshold.
        /// </summary>
        BinaryAllFaultedPoints = 1,

        /// <summary>
        /// Consider as anomaly if the number of points deviates from threshold are equal or greater than the specified value
        /// </summary>
        BinaryNumberOfFaultedPoints = 2,
    }

    public class ArmMetricMetadata
    {
        public string ArmResourceId { get; set; }

        public string MetricName { get; set; }

        public string MetricNamespace { get; set; }

        public TimeSpan BinSize { get; set; }

        public string Aggregation { get; set; }

        public SortedDictionary<string, string> Dimensions { get; set; }
    }
}