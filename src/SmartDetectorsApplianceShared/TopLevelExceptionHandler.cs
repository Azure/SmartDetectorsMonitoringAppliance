//-----------------------------------------------------------------------
// <copyright file="TopLevelExceptionHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance
{
    using System;
    using System.Linq;
    using Microsoft.Azure.WebJobs.Host;

    /// <summary>
    /// Utility class to perform top-level exception handling logic.
    /// </summary>
    public static class TopLevelExceptionHandler
    {
        /// <summary>
        /// Trace information for an unhandled exception.
        /// Each request handler should perform its logic within a try block, and call this handler within the catch block.
        /// </summary>
        /// <param name="ex">the exception caught at top level</param>
        /// <param name="tracer">Tracer that will be used to send telemetry on the failure</param>
        /// <param name="fallbackTracer">Fallback tracer, in case the tracer fails</param>
        public static void TraceUnhandledException(Exception ex, ITracer tracer, TraceWriter fallbackTracer)
        {
            // Trace the full exception details
            string errorToTrace = $"Top level exception: {ex}";

            try
            {
                // Write to trace
                tracer?.TraceError(errorToTrace);

                // Report the exception
                if (ex != null && tracer != null)
                {
                    AggregateException aggregateException = ex as AggregateException;
                    if (aggregateException != null)
                    {
                        aggregateException.InnerExceptions.ToList().ForEach(tracer.ReportException);
                    }
                    else
                    {
                        tracer.ReportException(ex);
                    }
                }

                // Flush all traces
                tracer?.Flush();
            }
            catch (Exception traceException)
            {
                try
                {
                    // Try to write to the fallback tracer
                    fallbackTracer?.Error($"Tracer failed - using fallback tracer. Tracer error: {traceException}");
                    fallbackTracer?.Error(errorToTrace);
                    fallbackTracer?.Flush();
                }
                catch (Exception fallbackTraceException)
                {
                    try
                    {
                        // Try to write to console
                        Console.WriteLine($"Tracer failed - using fallback tracer. Tracer error: {traceException}");
                        Console.WriteLine($"Fallback tracer failed - using console. Fallback tracer error: {fallbackTraceException}");
                        Console.WriteLine(errorToTrace);
                    }
                    catch (Exception)
                    {
                        // Just swallow, nothing better to do here..
                    }
                }
            }
        }
    }
}