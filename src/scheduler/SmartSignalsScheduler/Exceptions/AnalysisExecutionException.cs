//-----------------------------------------------------------------------
// <copyright file="AnalysisExecutionException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when we failed execute the signal when invoking the analysis flow.
    /// </summary>
    [Serializable]
    public class AnalysisExecutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecutionException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AnalysisExecutionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecutionException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public AnalysisExecutionException(string message, Exception e) : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecutionException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected AnalysisExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
