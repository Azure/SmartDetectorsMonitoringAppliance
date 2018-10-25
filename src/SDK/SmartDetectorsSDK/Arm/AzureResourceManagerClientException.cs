//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerClientException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Arm
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown whenever an error is encountered when calling the <see cref="IAzureResourceManagerClient"/> operations.
    /// </summary>
    [Serializable]
    public class AzureResourceManagerClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientException"/> class.
        /// </summary>
        public AzureResourceManagerClientException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AzureResourceManagerClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public AzureResourceManagerClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code that was received from ARM.</param>
        /// <param name="reasonPhrase">The reason phrase that was received from ARM</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public AzureResourceManagerClientException(HttpStatusCode statusCode, string reasonPhrase, Exception innerException)
            : this($"Failed calling ARM with status code:{statusCode} and reason phrase: {reasonPhrase}", innerException)
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerClientException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected AzureResourceManagerClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                this.StatusCode = (HttpStatusCode?)info.GetValue($"{nameof(AzureResourceManagerClientException)}_{nameof(this.StatusCode)}", typeof(HttpStatusCode?));
                this.ReasonPhrase = info.GetString($"{nameof(AzureResourceManagerClientException)}_{nameof(this.ReasonPhrase)}");
            }
            catch (InvalidCastException)
            {
            }
            catch (SerializationException)
            {
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code that was received from ARM (if any)
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the reason phrase that was received from ARM (if any)
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

            info.AddValue($"{nameof(AzureResourceManagerClientException)}_{nameof(this.StatusCode)}", this.StatusCode, typeof(HttpStatusCode?));
            info.AddValue($"{nameof(AzureResourceManagerClientException)}_{nameof(this.ReasonPhrase)}", this.ReasonPhrase, typeof(string));
        }
    }
}
