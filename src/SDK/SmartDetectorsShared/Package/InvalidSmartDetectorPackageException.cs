//-----------------------------------------------------------------------
// <copyright file="InvalidSmartDetectorPackageException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Package
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the Smart Detector package is invalid
    /// </summary>
    [Serializable]
    public class InvalidSmartDetectorPackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartDetectorPackageException"/> class.
        /// </summary>
        public InvalidSmartDetectorPackageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartDetectorPackageException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InvalidSmartDetectorPackageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartDetectorPackageException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public InvalidSmartDetectorPackageException(string message, Exception e)
            : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartDetectorPackageException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected InvalidSmartDetectorPackageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
