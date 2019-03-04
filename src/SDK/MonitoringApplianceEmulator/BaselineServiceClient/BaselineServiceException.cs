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
    public abstract class BaselineServiceException : Exception
    {
        protected BaselineServiceException(string message)
            : base(message)
        {
        }

        protected BaselineServiceException(string message, Exception innerException)
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
    public class BaselineServiceIsAnomalyException : BaselineServiceException
    {
        public BaselineServiceIsAnomalyException(string message)
            : base(message)
        {
        }

        public BaselineServiceIsAnomalyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceIsAnomalyException(SerializationInfo info, StreamingContext context)
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
    /// Exception thrown when GetQueriesForTrain request to Baseline Service fails
    /// </summary>
    public class BaselineServiceGetQueriesException : BaselineServiceException
    {
        public BaselineServiceGetQueriesException(string message, HttpStatusCode? responseStatusCode)
            : base(message)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        public BaselineServiceGetQueriesException(string message, Exception innerException, HttpStatusCode? responseStatusCode)
            : base(message, innerException)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        protected BaselineServiceGetQueriesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode? ResponseStatusCode { get; }
    }

    /// <summary>
    /// Exception thrown when GetPredictions request to Baseline Service fails
    /// </summary>
    public class BaselineServiceGetPredictionsRequestFailedException : BaselineServiceException
    {
        public BaselineServiceGetPredictionsRequestFailedException(string message, HttpStatusCode? responseStatusCode)
            : base(message)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        public BaselineServiceGetPredictionsRequestFailedException(string message, Exception innerException, HttpStatusCode? responseStatusCode)
            : base(message, innerException)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        protected BaselineServiceGetPredictionsRequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpStatusCode? ResponseStatusCode { get; }
    }

    /// <summary>
    /// Exception thrown when GetBaseline request to Baseline Service fails
    /// </summary>
    public class BaselineServiceGetBaselineRequestFailedException : BaselineServiceException
    {
        public BaselineServiceGetBaselineRequestFailedException(string message, HttpStatusCode? responseStatusCode)
            : base(message)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        public BaselineServiceGetBaselineRequestFailedException(string message, Exception innerException, HttpStatusCode? responseStatusCode)
            : base(message, innerException)
        {
            this.ResponseStatusCode = responseStatusCode;
        }

        protected BaselineServiceGetBaselineRequestFailedException(SerializationInfo info, StreamingContext context)
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

    /// <summary>
    /// Exception thrown when Dynamic Threshold train flow times out
    /// </summary>
    public class BaselineServiceTrainTimeoutException : BaselineServiceException
    {
        public BaselineServiceTrainTimeoutException(string message)
            : base(message)
        {
        }

        public BaselineServiceTrainTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceTrainTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when Dynamic Threshold getQueriesForTrain flow times out
    /// </summary>
    public class BaselineServiceGetQueriesTimeoutException : BaselineServiceException
    {
        public BaselineServiceGetQueriesTimeoutException(string message)
            : base(message)
        {
        }

        public BaselineServiceGetQueriesTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceGetQueriesTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when Dynamic Threshold training fails
    /// </summary>
    public class BaselineServiceTrainException : BaselineServiceException
    {
        public BaselineServiceTrainException(string message)
            : base(message)
        {
        }

        public BaselineServiceTrainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaselineServiceTrainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
