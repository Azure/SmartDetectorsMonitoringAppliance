//-----------------------------------------------------------------------
// <copyright file="TooManyResourcesException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where the list of resources exceeded the allowed limit.
    /// </summary>
    [Serializable]
    public class TooManyResourcesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TooManyResourcesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyResourcesException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected TooManyResourcesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}