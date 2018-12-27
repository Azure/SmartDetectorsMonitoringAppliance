//-----------------------------------------------------------------------
// <copyright file="ResolutionCheckNotSupportedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// This exception is thrown when a request for Alert resolution check is received for a Smart Detector which does not
    /// support this functionality.
    /// </summary>
    [Serializable]
    public class ResolutionCheckNotSupportedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionCheckNotSupportedException"/> class
        /// </summary>
        public ResolutionCheckNotSupportedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionCheckNotSupportedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public ResolutionCheckNotSupportedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionCheckNotSupportedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public ResolutionCheckNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionCheckNotSupportedException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ResolutionCheckNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
