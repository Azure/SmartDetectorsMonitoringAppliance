namespace TestChildProcess
{
    /// <summary>
    /// This enum is used to specify the expected behavior of the
    /// child process, to test various test scenarios.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// Run successfully
        /// </summary>
        Happy,

        /// <summary>
        /// Run successfully and return null
        /// </summary>
        Null,

        /// <summary>
        /// Throw an exception
        /// </summary>
        Exception,

        /// <summary>
        /// Run, wait for cancellation, and cancel gracefully
        /// </summary>
        Cancellation,

        /// <summary>
        /// Run and get stuck (ignore cancellation)
        /// </summary>
        Stuck,
    }
}