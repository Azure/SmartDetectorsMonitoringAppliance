//-----------------------------------------------------------------------
// <copyright file="ChildProcessFailedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception thrown whenever a child process fails (i.e. returns with exit code other than zero)
    /// </summary>
    [Serializable]
    public class ChildProcessFailedException : ChildProcessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessFailedException"/> class
        /// </summary>
        public ChildProcessFailedException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessFailedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public ChildProcessFailedException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessFailedException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The actual exception that was thrown when saving state</param>
        public ChildProcessFailedException(string message, Exception innerException)
            : base(message ?? "The child process has exited with an exception", innerException)
        {
            this.ExitCode = -1;
            this.Output = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessFailedException"/> class
        /// </summary>
        /// <param name="exitCode">The exit code of the child process</param>
        /// <param name="output">The output of the child process (as written to the output pipe).</param>
        public ChildProcessFailedException(int exitCode, string output)
            : base($"The child process has exited with an error code of {exitCode}")
        {
            this.ExitCode = exitCode;
            this.Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessFailedException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected ChildProcessFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                this.ExitCode = info.GetInt32($"{nameof(ChildProcessFailedException)}_{nameof(this.ExitCode)}");
                this.Output = info.GetString($"{nameof(ChildProcessFailedException)}_{nameof(this.Output)}");
            }
            catch (InvalidCastException)
            {
            }
            catch (SerializationException)
            {
            }
        }

        /// <summary>
        /// Gets or sets the exit code of the child process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the output of the child process (as written to the output pipe).
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/>
        /// about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue($"{nameof(ChildProcessFailedException)}_{nameof(this.ExitCode)}", this.ExitCode, typeof(int));
            info.AddValue($"{nameof(ChildProcessFailedException)}_{nameof(this.Output)}", this.Output, typeof(string));
        }
    }
}
