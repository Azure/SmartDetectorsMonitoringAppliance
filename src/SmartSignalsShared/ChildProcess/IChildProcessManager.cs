namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface of a class that manages running a task in a separate process
    /// </summary>
    public interface IChildProcessManager
    {
        /// <summary>
        /// Gets or sets the amount of time to wait on the child process till it gracefully cancels
        /// </summary>
        int CancellationGraceTimeInSeconds { get; set; }

        /// <summary>
        /// Gets the current status of running the child process
        /// </summary>
        RunChildProcessStatus CurrentStatus { get; }

        /// <summary>
        /// Gets the list of child process IDs created by this instance
        /// </summary>
        List<int> ChildProcessIds { get; }

        /// <summary>
        /// Runs a child process, synchronously, with the specified input.
        /// This method should be called by the parent process to start the child process.
        /// The child process should call <see cref="RunAndListenToParentAsync{TInput,TOutput}"/>,
        /// providing the main method that receives the input object and returns an object of
        /// type <see cref="TOutput"/>.
        /// </summary>
        /// <typeparam name="TOutput">The child process output type</typeparam>
        /// <param name="exePath">The child process' executable file path</param>
        /// <param name="input">The child process input</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="InvalidOperationException">The child process could not be started</exception>
        /// <exception cref="ChildProcessException">The child process failed - see InnerException ro details</exception>
        /// <returns>A <see cref="Task{TResult}"/>, returning the child process output</returns>
        Task<TOutput> RunChildProcessAsync<TOutput>(string exePath, object input, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the child process task. This method reads and validates the command line
        /// arguments, starts listening to the parent process (for cancellation/termination),
        /// runs the specified function, and returns the result to the parent process.
        /// Should be called by the child process when it starts.
        /// </summary>
        /// <typeparam name="TInput">The child process input type</typeparam>
        /// <typeparam name="TOutput">The child process output type</typeparam>
        /// <param name="args">The command line arguments</param>
        /// <param name="function">The function to run</param>
        /// <exception cref="ArgumentException">The wrong number of arguments was provided</exception>
        /// <returns>A <see cref="Task"/>, running the specified function and listening to the parent</returns>
        Task RunAndListenToParentAsync<TInput, TOutput>(string[] args, Func<TInput, CancellationToken, Task<TOutput>> function) where TOutput : class;
    }
}