//-----------------------------------------------------------------------
// <copyright file="IncompatibleResourceTypesException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;

    /// <summary>
    /// This exception is thrown when the requested resource type is not supported by the requested Smart Detector.
    /// </summary>
    [Serializable]
    public class IncompatibleResourceTypesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleResourceTypesException"/> class
        /// </summary>
        public IncompatibleResourceTypesException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleResourceTypesException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public IncompatibleResourceTypesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleResourceTypesException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public IncompatibleResourceTypesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleResourceTypesException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="requestResourceType">The requested resource type</param>
        /// <param name="smartDetectorManifest">The Smart Detector manifest</param>
        public IncompatibleResourceTypesException(ResourceType requestResourceType, SmartDetectorManifest smartDetectorManifest)
            : base($"Resource type {requestResourceType} is not supported by Smart Detector {smartDetectorManifest.Name}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleResourceTypesException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected IncompatibleResourceTypesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}