//-----------------------------------------------------------------------
// <copyright file="SmartDetectorNotFoundException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception thrown by the Smart Detector Repository when smart detector package does not exists.
    /// </summary>
    [Serializable]
    public class SmartDetectorNotFoundException : SmartDetectorRepositoryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorNotFoundException"/> class.
        /// </summary>
        public SmartDetectorNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SmartDetectorNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public SmartDetectorNotFoundException(string message, Exception e)
            : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartDetectorNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
