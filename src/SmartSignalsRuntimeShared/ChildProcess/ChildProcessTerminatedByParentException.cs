//-----------------------------------------------------------------------
// <copyright file="ChildProcessTerminatedByParentException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.ChildProcess
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
            : base("The child process failed to be cancelled - was terminated by parent")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessTerminatedByParentException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ChildProcessTerminatedByParentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}