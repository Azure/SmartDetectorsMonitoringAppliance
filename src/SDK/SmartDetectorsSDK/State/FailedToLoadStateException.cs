//-----------------------------------------------------------------------
// <copyright file="FailedToLoadStateException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception caused by issue with loading state.
    /// </summary>
    [Serializable]
    public class FailedToLoadStateException : StateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToLoadStateException"/> class
        /// </summary>
        public FailedToLoadStateException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToLoadStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public FailedToLoadStateException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToLoadStateException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when loading state</param>
        public FailedToLoadStateException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToLoadStateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when loading state</param>
        public FailedToLoadStateException(string message, Exception innerException)
            : base(message ?? "Unable to load state. See inner exception for more details.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToLoadStateException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        protected FailedToLoadStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}