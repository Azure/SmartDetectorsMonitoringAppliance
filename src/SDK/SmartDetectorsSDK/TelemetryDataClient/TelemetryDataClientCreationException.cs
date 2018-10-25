//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientCreationException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where a telemetry data client could not be created by the <see cref="IAnalysisServicesFactory"/>.
    /// A typical scenario is when the resources to be analyzed do not match the telemetry data client type.
    /// </summary>
    [Serializable]
    public class TelemetryDataClientCreationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientCreationException"/> class.
        /// </summary>
        public TelemetryDataClientCreationException()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientCreationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TelemetryDataClientCreationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientCreationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public TelemetryDataClientCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataClientCreationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected TelemetryDataClientCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}