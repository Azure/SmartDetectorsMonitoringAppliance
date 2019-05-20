//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
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
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class with a specified error message.
        /// </summary>
        public TelemetryDataClientException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TelemetryDataClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public TelemetryDataClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected TelemetryDataClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}