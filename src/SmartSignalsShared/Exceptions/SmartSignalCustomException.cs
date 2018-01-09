//-----------------------------------------------------------------------
// <copyright file="SmartSignalCustomException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception wraps an exception thrown by a smart signal.
    /// </summary>
    [Serializable]
    public class SmartSignalCustomException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalCustomException"/> class
        /// </summary>
        /// <param name="signalExceptionType">The original signal exception type</param>
        /// <param name="signalExceptionMessage">The original signal exception message</param>
        /// <param name="signalExceptionStackTrace">The original signal exception stack trace</param>
        public SmartSignalCustomException(string signalExceptionType, string signalExceptionMessage, string signalExceptionStackTrace) : base(signalExceptionMessage)
        {
            this.SignalExceptionType = signalExceptionType;
            this.SignalExceptionMessage = signalExceptionMessage;
            this.SignalExceptionStackTrace = signalExceptionStackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalCustomException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartSignalCustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.SignalExceptionType = info.GetString(nameof(this.SignalExceptionType));
            this.SignalExceptionMessage = info.GetString(nameof(this.SignalExceptionMessage));
            this.SignalExceptionStackTrace = info.GetString(nameof(this.SignalExceptionStackTrace));
        }

        /// <summary>
        /// Gets the original signal exception type name
        /// </summary>
        public string SignalExceptionType { get; }

        /// <summary>
        /// Gets the original signal exception message
        /// </summary>
        public string SignalExceptionMessage { get; }

        /// <summary>
        /// Gets the original signal exception stack trace
        /// </summary>
        public string SignalExceptionStackTrace { get; }

        /// <summary>
        /// Gets the object data from the serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.SignalExceptionType), this.SignalExceptionType);
            info.AddValue(nameof(this.SignalExceptionMessage), this.SignalExceptionMessage);
            info.AddValue(nameof(this.SignalExceptionStackTrace), this.SignalExceptionStackTrace);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a string representation of this exception, including the original exception's details.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return this.Message + Environment.NewLine +
                $"OriginalExceptionType: {this.SignalExceptionType}" + Environment.NewLine +
                $"OriginalExceptionStackTrace: {this.SignalExceptionStackTrace}" + Environment.NewLine +
                $"StackTrace: {this.StackTrace}";
        }
    }
}