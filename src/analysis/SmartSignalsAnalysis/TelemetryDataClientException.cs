//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This exception is thrown when there was an error running an analytics query.
    /// </summary>
    [Serializable]
    public class TelemetryDataClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class,
        /// with the details contained in the specified error response in OData JSON format.
        /// </summary>
        /// <param name="errorObject">The error object, in OData JSON format</param>
        /// <param name="query">The query that was run</param>
        public TelemetryDataClientException(JObject errorObject, string query)
            : this(errorObject, query, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected TelemetryDataClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class.
        /// with the details contained in the specified error response in OData JSON format.
        /// This private constructor is used to create both the outer and inner exceptions.
        /// </summary>
        /// <param name="errorObject">The error object</param>
        /// <param name="query">The query that was run</param>
        /// <param name="depth">The depth of the inner exception</param>
        private TelemetryDataClientException(JObject errorObject, string query, int depth)
            : base(BuildExceptionMessage(errorObject, query), BuildInnerException(errorObject["innererror"], depth))
        {
        }

        /// <summary>
        /// Builds the exception message from the specified error object and query.
        /// </summary>
        /// <param name="errorObject">The error object</param>
        /// <param name="query">The query</param>
        /// <returns>The error message</returns>
        private static string BuildExceptionMessage(JObject errorObject, string query)
        {
            if (errorObject == null)
            {
                return "Unspecified error";
            }

            string message = $"[{errorObject["code"]}] {errorObject["message"]}";
            if (!string.IsNullOrWhiteSpace(query))
            {
                message += Environment.NewLine + $" query = {query}";
            }

            return message;
        }

        /// <summary>
        /// Creates the inner exception from the specified inner error token.
        /// </summary>
        /// <param name="innerErrorObject">The inner error token</param>
        /// <param name="depth">The depth of the inner exception</param>
        /// <returns>The inner exception</returns>
        private static TelemetryDataClientException BuildInnerException(JToken innerErrorObject, int depth)
        {
            if (depth >= 5 || innerErrorObject == null)
            {
                return null;
            }
            
            return new TelemetryDataClientException((JObject)innerErrorObject, string.Empty, depth + 1);
        }
    }
}