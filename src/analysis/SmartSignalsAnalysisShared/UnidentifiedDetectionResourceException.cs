//-----------------------------------------------------------------------
// <copyright file="UnidentifiedDetectionResourceException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the detection's resource is not one of the resources provided to the signal.
    /// </summary>
    [Serializable]
    public class UnidentifiedDetectionResourceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedDetectionResourceException"/> class
        /// with the specified detection resource.
        /// </summary>
        /// <param name="detectionResource">The detection resource</param>
        public UnidentifiedDetectionResourceException(ResourceIdentifier detectionResource)
            : base($"Recieved a detection for resource \"{detectionResource.ResourceName}\", which did not match any of the resources provided to the signal")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedDetectionResourceException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected UnidentifiedDetectionResourceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}