//-----------------------------------------------------------------------
// <copyright file="FailedToSaveStateException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception caused by issue with saving state.
    /// </summary>
    [Serializable]
    public class FailedToSaveStateException : StateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToSaveStateException"/> class
        /// </summary>
        public FailedToSaveStateException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToSaveStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public FailedToSaveStateException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToSaveStateException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public FailedToSaveStateException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToSaveStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public FailedToSaveStateException(string message, Exception innerException)
            : base(message ?? "Unable to save state. See inner exception for more details.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToSaveStateException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        protected FailedToSaveStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
