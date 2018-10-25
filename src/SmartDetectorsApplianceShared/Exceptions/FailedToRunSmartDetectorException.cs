//-----------------------------------------------------------------------
// <copyright file="FailedToRunSmartDetectorException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception thrown whenever calling a Smart Detector's analyze method fails
    /// </summary>
    [Serializable]
    public class FailedToRunSmartDetectorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToRunSmartDetectorException"/> class
        /// </summary>
        public FailedToRunSmartDetectorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToRunSmartDetectorException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public FailedToRunSmartDetectorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToRunSmartDetectorException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="smartDetectorException">
        /// The exception that was thrown by the Smart Detector. Notice that this won't be added to the new exception's <see cref="Exception.InnerException"/>, since
        /// we must obfuscate the stack trace of exceptions thrown from within the detector code.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is the formal signature of the exception's constructor. The parameter is ignored on purpose.")]
        public FailedToRunSmartDetectorException(string message, Exception smartDetectorException)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToRunSmartDetectorException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected FailedToRunSmartDetectorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}