//-----------------------------------------------------------------------
// <copyright file="SmartDetectorLoadException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Loader
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown on failure to load a Smart Detector.
    /// </summary>
    [Serializable]
    public class SmartDetectorLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoadException"/> class.
        /// </summary>
        public SmartDetectorLoadException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoadException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name='message'>The message that explains the reason for the exception.</param>
        public SmartDetectorLoadException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoadException"/> class
        /// with a specified error message and a reference to an inner exception.
        /// </summary>
        /// <param name='message'>The message that explains the reason for the exception.</param>
        /// <param name='innerException'>The exception that is the cause of the current exception.
        /// If it is not a null reference, the current exception is raised in a catch block
        /// that handles the inner exception.</param>
        public SmartDetectorLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoadException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartDetectorLoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}