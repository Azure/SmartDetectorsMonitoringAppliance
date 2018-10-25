//-----------------------------------------------------------------------
// <copyright file="StateTooBigException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.State
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception caused by an attempt to store state object that is too big.
    /// </summary>
    [Serializable]
    public class StateTooBigException : StateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        public StateTooBigException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public StateTooBigException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateTooBigException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public StateTooBigException(string message, Exception innerException)
            : base(message ?? "Serialized state string is too long", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        /// <param name="serializedStateLength">The length of serialized state string</param>
        /// <param name="maxAllowedSerializedStateLength">Maximum allowed length of serialized state string</param>
        public StateTooBigException(
            long serializedStateLength,
            long maxAllowedSerializedStateLength)
            : base($"Serialized state string is too long ({serializedStateLength} characters), maximum allowed length is {maxAllowedSerializedStateLength}")
        {
            this.SerializedStateLength = serializedStateLength;
            this.MaxAllowedSerializedStateLength = maxAllowedSerializedStateLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTooBigException"/> class
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        protected StateTooBigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.SerializedStateLength = info.GetInt64(nameof(this.SerializedStateLength));
            this.MaxAllowedSerializedStateLength = info.GetInt64(nameof(this.MaxAllowedSerializedStateLength));
        }

        /// <summary>
        /// Gets the length of serialized state string
        /// </summary>
        public long SerializedStateLength { get; }

        /// <summary>
        /// Gets maximum allowed length of serialized state string
        /// </summary>
        public long MaxAllowedSerializedStateLength { get; }

        /// <summary>
        /// When overridden in a derived class, sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.SerializedStateLength), this.SerializedStateLength);
            info.AddValue(nameof(this.MaxAllowedSerializedStateLength), this.MaxAllowedSerializedStateLength);

            base.GetObjectData(info, context);
        }
    }
}
