//-----------------------------------------------------------------------
// <copyright file="ActivityLogException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when there is an error running the Activity Log query.
    /// </summary>
    [Serializable]
    public class ActivityLogException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogException"/> class
        /// </summary>
        public ActivityLogException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public ActivityLogException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public ActivityLogException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ActivityLogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
