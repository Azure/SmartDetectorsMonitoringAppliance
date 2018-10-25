//-----------------------------------------------------------------------
// <copyright file="MsiTokenException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when we fail to Acquire an MSI token.
    /// </summary>
    [Serializable]
    public class MsiTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsiTokenException"/> class.
        /// </summary>
        public MsiTokenException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiTokenException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public MsiTokenException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiTokenException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public MsiTokenException(string message, Exception e)
            : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiTokenException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected MsiTokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
