//-----------------------------------------------------------------------
// <copyright file="UnidentifiedResultItemResourceException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a signals' result item resource is not one of the resources provided to the signal.
    /// </summary>
    [Serializable]
    public class UnidentifiedResultItemResourceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedResultItemResourceException"/> class
        /// with the specified result item resource.
        /// </summary>
        /// <param name="resourceIdentifier">The result item resource</param>
        public UnidentifiedResultItemResourceException(ResourceIdentifier resourceIdentifier)
            : base($"Received a result item for resource \"{resourceIdentifier.ResourceName}\", which did not match any of the resources provided to the signal")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedResultItemResourceException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected UnidentifiedResultItemResourceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}