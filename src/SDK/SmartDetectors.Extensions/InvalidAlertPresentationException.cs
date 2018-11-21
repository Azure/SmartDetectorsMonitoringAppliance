//-----------------------------------------------------------------------
// <copyright file="InvalidAlertPresentationException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Extensions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the presentation information returned by an
    /// alert is invalid
    /// </summary>
    [Serializable]
    public class InvalidAlertPresentationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class.
        /// </summary>
        public InvalidAlertPresentationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The error message</param>
        public InvalidAlertPresentationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public InvalidAlertPresentationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected InvalidAlertPresentationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}