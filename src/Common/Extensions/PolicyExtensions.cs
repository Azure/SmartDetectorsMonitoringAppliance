//-----------------------------------------------------------------------
// <copyright file="PolicyExtensions.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Polly;

    /// <summary>
    /// Extension methods for policy objects
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// Runs an asynchronous dependency operation, using the specified policy, sends
        /// telemetry information about it using the specified tracer, and returns the
        /// operation result.
        /// Any exception in the dependency call would be propagated to the caller.
        /// </summary>
        /// <typeparam name="T">The operation result type</typeparam>
        /// <param name="policy">The policy</param>
        /// <param name="tracer">The tracer</param>
        /// <param name="dependencyName">The dependency name.</param>
        /// <param name="commandName">The command name</param>
        /// <param name="dependencyCall">The dependency call action</param>
        /// <param name="extractMetricsFromResultFunc">Function that extracts metrics from the result</param>
        /// <param name="properties">Named string values you can use to classify and filter dependencies</param>
        /// <returns>A task running the asynchronous operation and returning its result</returns>
        public static async Task<T> RunAndTrackDependencyAsync<T>(
            this Policy policy,
            ITracer tracer,
            string dependencyName,
            string commandName,
            Func<Task<T>> dependencyCall,
            Func<T, IDictionary<string, double>> extractMetricsFromResultFunc = null,
            IDictionary<string, string> properties = null)
        {
            DateTime startTime = DateTime.UtcNow;
            bool success = false;
            IDictionary<string, double> metrics = null;
            try
            {
                T result = await policy.ExecuteAsync(dependencyCall);

                metrics = extractMetricsFromResultFunc?.Invoke(result);
                success = true;
                return result;
            }
            finally
            {
                tracer.TrackDependency(dependencyName, commandName, startTime, DateTime.UtcNow - startTime, success, metrics, properties);
            }
        }

        /// <summary>
        /// Creates a <see cref="Policy"/> instance with default exponential retry policy.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="dependencyName">The dependency name</param>
        /// <param name="retryCount">The retry count (3 by default)</param>
        /// <returns>The retry policy</returns>
        public static Policy CreateDefaultPolicy(ITracer tracer, string dependencyName, int retryCount = 3)
        {
            return Policy.Handle<Exception>(ex => true).WaitAndRetryAsync(
                retryCount,
                (i) => TimeSpan.FromSeconds(Math.Pow(2, i)),
                (exception, span, context) => tracer.TraceError($"Failed accessing {dependencyName} on {exception.Message}, retry {Math.Log(span.Seconds, 2)} out of {retryCount}"));
        }
    }
}