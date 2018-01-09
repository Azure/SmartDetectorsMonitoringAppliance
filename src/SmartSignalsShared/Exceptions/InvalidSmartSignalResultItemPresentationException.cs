//-----------------------------------------------------------------------
// <copyright file="InvalidSmartSignalResultItemPresentationException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the presentation information returned by a
    /// smart signal result item is invalid
    /// </summary>
    [Serializable]
    public class InvalidSmartSignalResultItemPresentationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartSignalResultItemPresentationException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The error message</param>
        public InvalidSmartSignalResultItemPresentationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartSignalResultItemPresentationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected InvalidSmartSignalResultItemPresentationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}