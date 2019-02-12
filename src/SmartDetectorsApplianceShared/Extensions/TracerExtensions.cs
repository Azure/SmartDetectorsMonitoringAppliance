//-----------------------------------------------------------------------
// <copyright file="TracerExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Extensions
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for <see cref="IExtendedTracer"/> objects
    /// </summary>
    public static class TracerExtensions
    {
        private const string PerformanceCounterMetricPrefix = "SmartDetectorsAppCounter";

        /// <summary>
        /// Trace relevant performance counters as metrics.
        /// </summary>
        /// <param name="tracer">The tracer to use</param>
        public static void TraceAppCounters(this IExtendedTracer tracer)
        {
            string countersJson = null;
            try
            {
                countersJson = Environment.GetEnvironmentVariable("WEBSITE_COUNTERS_APP");
                if (string.IsNullOrEmpty(countersJson))
                {
                    tracer.TraceWarning("Failed to trace counters: environment variable value is empty");
                    return;
                }

                // TEMP: need to parse this specially to work around bug where
                // sometimes an extra garbage character occurs after the terminal
                // brace
                int idx = countersJson.LastIndexOf('}');
                if (idx > 0)
                {
                    countersJson = countersJson.Substring(0, idx + 1);
                }

                JObject countersObject = (JObject)JsonConvert.DeserializeObject(countersJson);
                foreach (var counter in countersObject)
                {
                    // The metric name is the counter's category and name, excluding non-letters
                    string metricName = $"{PerformanceCounterMetricPrefix}_{counter.Key}";

                    // Try to parse the value
                    if (!double.TryParse(counter.Value.ToString(), out double metricValue))
                    {
                        tracer.TraceWarning($"Failed to trace counter {counter.Key}, value {counter.Value}");
                        continue;
                    }

                    // Report the metric
                    tracer.ReportMetric(metricName, metricValue);
                }
            }
            catch (Exception e)
            {
                tracer.TraceWarning($"Failed to trace counters: countersJson = {countersJson}, error message = {e.Message}");
            }
        }
    }
}