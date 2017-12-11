namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess
{
    /// <summary>
    /// Represents the status of running a child process. Used by the
    /// parent process to track the progress of the child process run.
    /// </summary>
    public enum RunChildProcessStatus
    {
        /// <summary>
        /// No child process was run
        /// </summary>
        None,

        /// <summary>
        /// Initializing the run of the child process
        /// </summary>
        Initializing,

        /// <summary>
        /// The child process was run, waiting for it to exit
        /// </summary>
        WaitingForProcessToExit,

        /// <summary>
        /// The child process exited - reading its output and finalizing
        /// </summary>
        Finalizing,

        /// <summary>
        /// The child process failed
        /// </summary>
        Failed,

        /// <summary>
        /// The child process was canceled
        /// </summary>
        Canceled,

        /// <summary>
        /// The child process completed successfully
        /// </summary>
        Completed,
    }
}