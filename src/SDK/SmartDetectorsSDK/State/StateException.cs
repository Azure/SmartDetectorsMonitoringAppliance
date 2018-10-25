//-----------------------------------------------------------------------
// <copyright file="StateException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a base class for all state related exceptions
    /// </summary>
    [Serializable]
    public class StateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateException"/> class
        /// </summary>
        public StateException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public StateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateException(Exception innerException)
            : base(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected StateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}