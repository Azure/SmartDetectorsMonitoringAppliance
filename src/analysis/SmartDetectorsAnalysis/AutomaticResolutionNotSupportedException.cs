//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionNotSupportedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// This exception is thrown when a request for Alert automatic resolution check is received for a Smart Detector which does not
    /// support this functionality.
    /// </summary>
    [Serializable]
    public class AutomaticResolutionNotSupportedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionNotSupportedException"/> class
        /// </summary>
        public AutomaticResolutionNotSupportedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionNotSupportedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public AutomaticResolutionNotSupportedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionNotSupportedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public AutomaticResolutionNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionNotSupportedException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected AutomaticResolutionNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
