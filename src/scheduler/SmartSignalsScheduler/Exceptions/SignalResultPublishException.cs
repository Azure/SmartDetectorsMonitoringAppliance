//-----------------------------------------------------------------------
// <copyright file="SignalResultPublishException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when we publishing the signal results fails.
    /// </summary>
    [Serializable]
    public class SignalResultPublishException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultPublishException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SignalResultPublishException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultPublishException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public SignalResultPublishException(string message, Exception e) : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultPublishException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SignalResultPublishException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
