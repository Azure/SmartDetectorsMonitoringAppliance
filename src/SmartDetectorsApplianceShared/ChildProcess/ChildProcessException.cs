//-----------------------------------------------------------------------
// <copyright file="ChildProcessException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a child process fails.
    /// </summary>
    [Serializable]
    public class ChildProcessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessException"/> class
        /// </summary>
        public ChildProcessException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public ChildProcessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="e">The exception thrown by the child process</param>
        public ChildProcessException(string message, Exception e)
            : base(message, e)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ChildProcessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}