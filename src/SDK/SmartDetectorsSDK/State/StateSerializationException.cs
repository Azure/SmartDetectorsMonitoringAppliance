// <copyright file="StateSerializationException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception caused by state serialization issue.
    /// </summary>
    [Serializable]
    public class StateSerializationException : StateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateSerializationException"/> class
        /// </summary>
        public StateSerializationException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSerializationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public StateSerializationException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSerializationException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateSerializationException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSerializationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateSerializationException(string message, Exception innerException)
            : base(message ?? "State serialization/deserialization failed. See inner exception for more details. If you would like to use custom serialization logic - serialize the state in client code and store it as a 'System.String'.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSerializationException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        protected StateSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
