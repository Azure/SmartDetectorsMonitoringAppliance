//-----------------------------------------------------------------------
// <copyright file="UnidentifiedAlertResourceTypeException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// This exception is thrown when an alert resource type is not one of the types supported by the Smart Detector.
    /// </summary>
    [Serializable]
    public class UnidentifiedAlertResourceTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// </summary>
        public UnidentifiedAlertResourceTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public UnidentifiedAlertResourceTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public UnidentifiedAlertResourceTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// with the specified alert resource.
        /// </summary>
        /// <param name="resourceIdentifier">The alert resource</param>
        public UnidentifiedAlertResourceTypeException(ResourceIdentifier resourceIdentifier)
            : base($"Received an alert for resource \"{resourceIdentifier.ResourceName}\", of type {resourceIdentifier.ResourceType}, which did not match any of the resource types supported by the Smart Detector")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected UnidentifiedAlertResourceTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}