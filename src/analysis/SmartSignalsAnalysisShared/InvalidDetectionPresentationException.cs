namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the presentation information returned by a
    /// smart signal detection is invalid
    /// </summary>
    public class InvalidDetectionPresentationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDetectionPresentationException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The error message</param>
        public InvalidDetectionPresentationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDetectionPresentationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected InvalidDetectionPresentationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}