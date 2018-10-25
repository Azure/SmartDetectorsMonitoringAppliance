//-----------------------------------------------------------------------
// <copyright file="FailedToDeleteStateException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception caused by issue with deleting state.
    /// </summary>
    [Serializable]
    public class FailedToDeleteStateException : StateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToDeleteStateException"/> class
        /// </summary>
        public FailedToDeleteStateException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToDeleteStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public FailedToDeleteStateException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToDeleteStateException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when deleting state</param>
        public FailedToDeleteStateException(Exception innerException)
            : base(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToDeleteStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when deleting state</param>
        public FailedToDeleteStateException(string message, Exception innerException)
            : base(message ?? "Unable to delete state. See inner exception for more details.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToDeleteStateException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        protected FailedToDeleteStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}