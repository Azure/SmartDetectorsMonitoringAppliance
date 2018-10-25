//-----------------------------------------------------------------------
// <copyright file="SmartDetectorsManagementApiException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception thrown from the management API logic
    /// </summary>
    [Serializable]
    public class SmartDetectorsManagementApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// </summary>
        public SmartDetectorsManagementApiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public SmartDetectorsManagementApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public SmartDetectorsManagementApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// with a specified error message and status code.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="statusCode">The HTTP status code that represents the exception.</param>
        public SmartDetectorsManagementApiException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// with a specified error message and status code.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="exception">The inner exception.</param>
        /// <param name="statusCode">The HTTP status code that represents the exception.</param>
        public SmartDetectorsManagementApiException(string message, Exception exception, HttpStatusCode statusCode)
            : base(message, exception)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartDetectorsManagementApiException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        /// Gets the status code that represents the exception
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
