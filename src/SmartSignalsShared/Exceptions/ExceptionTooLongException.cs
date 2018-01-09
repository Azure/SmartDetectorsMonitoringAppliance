//-----------------------------------------------------------------------
// <copyright file="ExceptionTooLongException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where an exception is thrown, but the exception is too long
    /// and cannot be tracked by the tracer. Instead, an <see cref="ExceptionTooLongException"/> should be reported.
    /// </summary>
    [Serializable]
    public class ExceptionTooLongException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTooLongException"/> class from the original exception.
        /// </summary>
        /// <param name="e">The original exception that was thrown.</param>
        /// <param name="referenceId">A reference ID used to correlate the exception details in the traces</param>
        public ExceptionTooLongException(Exception e, string referenceId)
            : base(CreateMessage(e, referenceId))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTooLongException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ExceptionTooLongException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Create the exception message.
        /// The message contains the exception type, reference Id, and the beginning of the original message.
        /// </summary>
        /// <param name="e">The exception</param>
        /// <param name="referenceId">The reference Id</param>
        /// <returns>The message</returns>
        private static string CreateMessage(Exception e, string referenceId)
        {
            return $"Exception {e.GetType().Name} is too long - refer to the trace for more details, reference Id = {referenceId}, Message = {e.Message.Substring(0, 1000)}";
        }
    }
}