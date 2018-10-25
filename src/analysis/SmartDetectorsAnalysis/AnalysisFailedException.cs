//-----------------------------------------------------------------------
// <copyright file="AnalysisFailedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception thrown when the Smart Detector analysis fails
    /// </summary>
    [Serializable]
    public class AnalysisFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedException"/> class.
        /// </summary>
        public AnalysisFailedException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AnalysisFailedException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public AnalysisFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.StatusCode = HttpStatusCode.InternalServerError;
            this.ReasonPhrase = "Analysis failed due to internal server error";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code that describes the analysis failure.</param>
        /// <param name="reasonPhrase">The reason phrase that describes the analysis failure.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AnalysisFailedException(HttpStatusCode statusCode, string reasonPhrase, Exception innerException)
            : this($"Failed running Smart Detector Analysis status code:{statusCode} and reason phrase: {reasonPhrase}", innerException)
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisFailedException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected AnalysisFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                this.StatusCode = (HttpStatusCode)info.GetValue($"{nameof(AnalysisFailedException)}_{nameof(this.StatusCode)}", typeof(HttpStatusCode));
                this.ReasonPhrase = info.GetString($"{nameof(AnalysisFailedException)}_{nameof(this.ReasonPhrase)}");
            }
            catch (InvalidCastException)
            {
            }
            catch (SerializationException)
            {
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code that describes the analysis failure.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the reason phrase that was describes the analysis failure.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/>
        /// about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue($"{nameof(AnalysisFailedException)}_{nameof(this.StatusCode)}", this.StatusCode, typeof(HttpStatusCode));
            info.AddValue($"{nameof(AnalysisFailedException)}_{nameof(this.ReasonPhrase)}", this.ReasonPhrase, typeof(string));
        }
    }
}
