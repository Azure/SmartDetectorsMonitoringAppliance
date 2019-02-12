//-----------------------------------------------------------------------
// <copyright file="PolicyExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
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
            IExtendedTracer tracer,
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
            this Policy<T> policy,
            IExtendedTracer tracer,
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
        public static Policy CreateDefaultPolicy(IExtendedTracer tracer, string dependencyName, int retryCount = 3)
        {
            return Policy.Handle<Exception>(ex => !(ex is TaskCanceledException)).WaitAndRetryAsync(
                retryCount,
                (i) => TimeSpan.FromSeconds(Math.Pow(2, i)),
                (exception, span, context) => tracer.TraceError($"Failed accessing {dependencyName} on {exception.Message}, retry {Math.Log(span.Seconds, 2)} out of {retryCount}"));
        }

        /// <summary>
        /// Creates a <see cref="Policy"/> instance that retries on transient HTTP errors with default exponential retry policy.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="dependencyName">The dependency name</param>
        /// <param name="retryCount">The retry count (3 by default)</param>
        /// <returns>The retry policy</returns>
        public static Policy<HttpResponseMessage> CreateTransientHttpErrorPolicy(IExtendedTracer tracer, string dependencyName, int retryCount = 3)
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(response => response.StatusCode >= HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(
                    retryCount,
                    i => TimeSpan.FromSeconds(Math.Pow(2, i)),
                    (response, span, context) =>
                    {
                        if (response.Exception != null)
                        {
                            tracer.TraceError($"Failed accessing {dependencyName} on exception {response.Exception.Message}, retry {Math.Log(span.Seconds, 2)} out of {retryCount}");
                        }
                        else if (response.Result != null)
                        {
                            tracer.TraceError($"Failed accessing {dependencyName} on HTTP error {response.Result.StatusCode}, retry {Math.Log(span.Seconds, 2)} out of {retryCount}");
                        }
                        else
                        {
                            tracer.TraceError($"Failed accessing {dependencyName} for unknown reason, retry {Math.Log(span.Seconds, 2)} out of {retryCount}");
                        }
                    });
        }
    }
}