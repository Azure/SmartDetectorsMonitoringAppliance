//-----------------------------------------------------------------------
// <copyright file="AutomaticResolutionStateNotFoundException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a request for Alert automatic resolution check is received but the state needed
    /// to handle the request is not found in the state repository.
    /// </summary>
    [Serializable]
    public class AutomaticResolutionStateNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionStateNotFoundException"/> class
        /// </summary>
        public AutomaticResolutionStateNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionStateNotFoundException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public AutomaticResolutionStateNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionStateNotFoundException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public AutomaticResolutionStateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticResolutionStateNotFoundException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected AutomaticResolutionStateNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
