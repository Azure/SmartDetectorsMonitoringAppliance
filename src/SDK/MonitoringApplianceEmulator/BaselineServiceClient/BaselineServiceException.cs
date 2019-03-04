//-----------------------------------------------------------------------
// <copyright file="BaselineServiceException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable CS1591, SA1600, SA1402, CA1032, CA2237

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for BaselineServiceExceptions
    /// </summary>
    public class BaselineServiceException : Exception
    {
        public BaselineServiceException(string message)
            : base(message)
        {
        }

        public BaselineServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when IsAnomaly flow fails
    /// </summary>
    public class BaselineServiceModelMissingException : BaselineServiceException
    {
        public BaselineServiceModelMissingException(string message)
            : base(message)
        {
        }

        public BaselineServiceModelMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceModelMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when IsAnomaly flow fails
    /// </summary>
    public class BaselineServiceModelUntrainedException : BaselineServiceException
    {
        public BaselineServiceModelUntrainedException(string message)
            : base(message)
        {
        }

        public BaselineServiceModelUntrainedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceModelUntrainedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when anomaly request to Baseline Service fails
    /// </summary>
    public class BaselineServiceRequestFailedException : BaselineServiceException
    {
        public BaselineServiceRequestFailedException(string message, HttpStatusCode? responseStatusCode)
            : base(message)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        public BaselineServiceRequestFailedException(string message, Exception innerException, HttpStatusCode? responseStatusCode)
            : base(message, innerException)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        protected BaselineServiceRequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode? ResponseStatusCode { get; }
    }

    /// <summary>
    /// Exception thrown when Dynamic Threshold flow times out
    /// </summary>
    public class BaselineServiceTimeoutException : BaselineServiceException
    {
        public BaselineServiceTimeoutException(string message)
            : base(message)
        {
        }

        public BaselineServiceTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
