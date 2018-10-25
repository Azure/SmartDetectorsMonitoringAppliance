//-----------------------------------------------------------------------
// <copyright file="ChildProcessTerminatedByParentException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when a child process was forcefully terminated by the parent process.
    /// </summary>
    [Serializable]
    public class ChildProcessTerminatedByParentException : ChildProcessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessTerminatedByParentException"/> class
        /// </summary>
        public ChildProcessTerminatedByParentException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessTerminatedByParentException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public ChildProcessTerminatedByParentException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessTerminatedByParentException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public ChildProcessTerminatedByParentException(string message, Exception innerException)
            : base(message ?? "The child process failed to be cancelled - was terminated by parent", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessTerminatedByParentException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ChildProcessTerminatedByParentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}