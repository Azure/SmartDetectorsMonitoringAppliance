//-----------------------------------------------------------------------
// <copyright file="SmartSignalRepositoryException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception thrown by the Smart Signal Repository.
    /// </summary>
    [Serializable]
    public class SmartSignalRepositoryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRepositoryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SmartSignalRepositoryException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRepositoryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public SmartSignalRepositoryException(string message, Exception e) : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRepositoryException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartSignalRepositoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
