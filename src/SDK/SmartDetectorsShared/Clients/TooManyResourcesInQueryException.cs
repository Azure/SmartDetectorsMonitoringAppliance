//-----------------------------------------------------------------------
// <copyright file="TooManyResourcesInQueryException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where there are too many resources in the query.
    /// </summary>
    [Serializable]
    public class TooManyResourcesInQueryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesInQueryException"/> class.
        /// </summary>
        public TooManyResourcesInQueryException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesInQueryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TooManyResourcesInQueryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesInQueryException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public TooManyResourcesInQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesInQueryException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected TooManyResourcesInQueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}