//-----------------------------------------------------------------------
// <copyright file="DetectorDataNotReadyException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception used by the Smart Detector to indicate that the data it need for analysis
    /// is not ready yet. The Runtime Environment may use <see cref="ShouldRetryAfter"/> to
    /// reschedule the analysis, but in any case throwing this exception will not register as
    /// a failure to run the Smart Detector.
    /// </summary>
    [Serializable]
    public class DetectorDataNotReadyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        public DetectorDataNotReadyException()
        {
            this.ShouldRetryAfter = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public DetectorDataNotReadyException(string message)
            : base(message)
        {
            this.ShouldRetryAfter = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public DetectorDataNotReadyException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.ShouldRetryAfter = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        /// <param name="shouldRetryAfter">
        /// A value indicating when the Runtime Environment may retry running the analysis again.
        /// </param>
        public DetectorDataNotReadyException(TimeSpan shouldRetryAfter)
        {
            this.ShouldRetryAfter = shouldRetryAfter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        /// <param name="shouldRetryAfter">
        /// A value indicating when the Runtime Environment may retry running the analysis again.
        /// </param>
        /// <param name="message">The exception message</param>
        public DetectorDataNotReadyException(TimeSpan shouldRetryAfter, string message)
            : this(message)
        {
            this.ShouldRetryAfter = shouldRetryAfter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// </summary>
        /// <param name="shouldRetryAfter">
        /// A value indicating when the Runtime Environment may retry running the analysis again.
        /// </param>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public DetectorDataNotReadyException(TimeSpan shouldRetryAfter, string message, Exception innerException)
            : this(message, innerException)
        {
            this.ShouldRetryAfter = shouldRetryAfter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorDataNotReadyException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected DetectorDataNotReadyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            try
            {
                this.ShouldRetryAfter = (TimeSpan)serializationInfo.GetValue("DetectorDataNotReadyException_ShouldRetryAfter", typeof(TimeSpan));
            }
            catch (InvalidCastException)
            {
            }
            catch (SerializationException)
            {
            }
        }

        /// <summary>
        /// Gets a value indicating when the Runtime Environment may retry
        /// running the analysis again.
        /// </summary>
        public TimeSpan ShouldRetryAfter { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/>
        /// about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("DetectorDataNotReadyException_ShouldRetryAfter", this.ShouldRetryAfter, typeof(Type));
        }
    }
}
