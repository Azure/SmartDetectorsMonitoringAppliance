namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient
{
    using System;

    /// <summary>
    /// This exception is thrown when the we failed to query the AI Rest API
    /// </summary>
    public class ApplicationInsightsClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient.ApplicationInsightsClientException" /> class
        /// with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message</param>
        public ApplicationInsightsClientException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient.ApplicationInsightsClientException" /> class
        /// with the specified error message and exception.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The original exception</param>
        public ApplicationInsightsClientException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
