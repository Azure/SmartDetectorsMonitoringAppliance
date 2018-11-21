//-----------------------------------------------------------------------
// <copyright file="ChildProcessManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Loader;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Newtonsoft.Json;

    /// <summary>
    /// An implementation of the <see cref="IChildProcessManager"/> interface.
    /// This class manages running a task in a separate process, including:
    /// * Passing the parameters from the parent process to the child process
    /// * Passing the result from the child process to the parent process
    /// * Cancellation
    /// * Error handling
    /// </summary>
    public class ChildProcessManager : IChildProcessManager
    {
        #region Fields, Constructors, and Properties

        private const string CancellationString = "CANCEL";
        private const string TempSubFolderName = "ChildProcessManagerTemp";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
        };

        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildProcessManager"/> class
        /// </summary>
        /// <param name="tracer">The tracer</param>
        public ChildProcessManager(IExtendedTracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
        }

        /// <summary>
        /// Gets or sets the amount of time to wait on the child process till it gracefully cancels
        /// </summary>
        public int CancellationGraceTimeInSeconds { get; set; } = (int)TimeSpan.FromMinutes(4).TotalSeconds;

        /// <summary>
        /// Gets the current status of running the child process
        /// </summary>
        public RunChildProcessStatus CurrentStatus { get; private set; } = RunChildProcessStatus.None;

        /// <summary>
        /// Gets the list of child process IDs created by this instance
        /// </summary>
        public List<int> ChildProcessIds { get; } = new List<int>();

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a tracer object for the child process, based on the command line arguments received from the parent process.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The tracer instance</returns>
        public IExtendedTracer CreateTracerForChildProcess(string[] args)
        {
            ChildProcessArguments arguments = ChildProcessArguments.FromCommandLineArguments(args);

            // Get sessionId from the arguments and create tracer
            IExtendedTracer tracerForChildProcess = TracerFactory.Create(arguments.SessionId, null, true);

            // Get custom dimensions from the arguments - do not override existing properties
            IReadOnlyDictionary<string, string> existingProperties = tracerForChildProcess.GetCustomProperties();
            foreach (var kv in arguments.CustomTracerProperties)
            {
                if (!existingProperties.ContainsKey(kv.Key))
                {
                    tracerForChildProcess.AddCustomProperty(kv.Key, kv.Value);
                }
            }

            return tracerForChildProcess;
        }

        /// <summary>
        /// Creates a smart detector loader object for the child process, based on the command line arguments received from the parent process.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="tracerForLoader">The tracer to use</param>
        /// <returns>The smart detector loader instance</returns>
        public ISmartDetectorLoader CreateLoaderForChildProcess(string[] args, IExtendedTracer tracerForLoader)
        {
            // Get the temp folder name from the arguments and create the loader
            ChildProcessArguments arguments = ChildProcessArguments.FromCommandLineArguments(args);
            return new SmartDetectorLoader(arguments.TempFolder, tracerForLoader);
        }

        /// <summary>
        /// Runs a child process, synchronously, with the specified input.
        /// This method should be called by the parent process. It starts the child process, providing it
        /// with specific command line arguments that will allow the child process to support cancellation
        /// and error handling.
        /// The child process should call <see cref="RunAndListenToParentAsync{TInput,TOutput}"/>, provide
        /// it with the command line arguments and the main method that receives the input object and returns
        /// an object of type <typeparamref name="TOutput"/>.
        /// </summary>
        /// <example>
        /// Parent process:
        /// <code>
        /// private async cTask&lt;OutputData&gt; RunInChildProcess(string childProcessName, InputData input, IExtendedTracer tracer, CancellationToken cancellationToken)
        /// {
        ///     IChildProcessManager childProcessManager = new ChildProcessManager();
        ///     OutputData output = await childProcessManager.RunChildProcessAsync&lt;OutputData&gt;(childProcessName, input, tracer, cancellationToken);
        ///     return output;
        /// }
        /// </code>
        /// Child process:
        /// <code>
        /// public static void Main(string[] args)
        /// {
        ///     IExtendedTracer tracer;
        ///     // Initialize tracer...
        ///
        ///     IChildProcessManager childProcessManager = new ChildProcessManager();
        ///     childProcessManager.RunAndListenToParentAsync&lt;InputData, OutputData&gt;(args, MainFunction, tracer).Wait();
        /// }
        ///
        /// private static OutputData MainFunction(InputData input, CancellationToken cancellationToken)
        /// {
        ///     // ...
        ///
        ///     return output;
        /// }
        /// </code>
        /// </example>
        /// <typeparam name="TOutput">The child process output type</typeparam>
        /// <param name="exePath">The child process' executable file path</param>
        /// <param name="input">The child process input</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="InvalidOperationException">The child process could not be started</exception>
        /// <exception cref="ChildProcessException">The child process failed - see InnerException for details</exception>
        /// <returns>A <see cref="Task{TResult}"/>, returning the child process output</returns>
        public async Task<TOutput> RunChildProcessAsync<TOutput>(string exePath, object input, CancellationToken cancellationToken)
        {
            this.CurrentStatus = RunChildProcessStatus.Initializing;
            this.tracer.TraceInformation($"Starting to run child process {exePath}");

            // Create a temporary folder for the child process
            string tempFolder = FileSystemExtensions.CreateTempFolder(TempSubFolderName);
            this.tracer.TraceInformation($"Created temporary folder for child process: {tempFolder}");

            try
            {
                // The pipe from the parent to the child is used to pass a cancellation instruction
                // The pipe from the child to the parent is used to pass the child process output
                using (AnonymousPipeServerStream pipeParentToChild = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
                {
                    using (AnonymousPipeServerStream pipeChildToParent = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
                    {
                        using (Process childProcess = new Process())
                        {
                            // Write the output to the pipe
                            await this.WriteToStream(input, pipeParentToChild, cancellationToken);

                            // Get pipe handles
                            string pipeParentToChildHandle = pipeParentToChild.GetClientHandleAsString();
                            string pipeChildToParentHandle = pipeChildToParent.GetClientHandleAsString();

                            // Prepare command line arguments
                            ChildProcessArguments arguments = new ChildProcessArguments(
                                pipeParentToChildHandle,
                                pipeChildToParentHandle,
                                this.tracer.SessionId,
                                this.tracer.GetCustomProperties(),
                                tempFolder);

                            // Setup the child process
                            childProcess.StartInfo = new ProcessStartInfo(exePath)
                            {
                                Arguments = ChildProcessArguments.ToCommandLineArguments(arguments),
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardError = true
                            };

                            // Start the child process
                            Stopwatch sw = Stopwatch.StartNew();
                            childProcess.Start();
                            this.tracer.TraceInformation($"Started to run child process '{Path.GetFileName(exePath)}', process ID {childProcess.Id}");
                            this.CurrentStatus = RunChildProcessStatus.WaitingForProcessToExit;
                            this.ChildProcessIds.Add(childProcess.Id);

                            // Dispose the local copy of the client handle
                            pipeParentToChild.DisposeLocalCopyOfClientHandle();
                            pipeChildToParent.DisposeLocalCopyOfClientHandle();

                            // Wait for the child process to finish
                            bool wasChildTerminatedByParent = false;
                            MemoryStream outputStream = new MemoryStream();
                            using (cancellationToken.Register(() => { this.CancelChildProcess(childProcess, pipeParentToChild, ref wasChildTerminatedByParent); }))
                            {
                                // Read the child's output
                                // We do not use the cancellation token here - we want to wait for the child to gracefully cancel
                                await pipeChildToParent.CopyToAsync(outputStream, 2048, default(CancellationToken));

                                // Ensure the child existed
                                childProcess.WaitForExit();
                            }

                            this.CurrentStatus = RunChildProcessStatus.Finalizing;
                            sw.Stop();
                            this.tracer.TraceInformation($"Process {exePath} completed, duration {sw.ElapsedMilliseconds / 1000}s, exit code {childProcess.ExitCode}");

                            // If the child process was terminated by the parent, throw appropriate exception
                            if (wasChildTerminatedByParent)
                            {
                                throw new ChildProcessTerminatedByParentException();
                            }

                            // If the child process has exited with an error code, throw appropriate exception
                            if (childProcess.ExitCode != 0)
                            {
                                // This read ignores the cancellation token - if there was a cancellation, the process output will contain the appropriate exception
                                outputStream.Seek(0, SeekOrigin.Begin);
                                string processOutput = await this.ReadFromStream<string>(outputStream, default(CancellationToken));
                                throw new ChildProcessFailedException(childProcess.ExitCode, processOutput);
                            }

                            // Read the process result from the stream
                            outputStream.Seek(0, SeekOrigin.Begin);
                            TOutput processResult = await this.ReadFromStream<TOutput>(outputStream, cancellationToken);

                            // Return process result
                            this.CurrentStatus = RunChildProcessStatus.Completed;
                            return processResult;
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.CurrentStatus = cancellationToken.IsCancellationRequested ? RunChildProcessStatus.Canceled : RunChildProcessStatus.Failed;
                throw;
            }
            finally
            {
                FileSystemExtensions.TryDeleteFolder(tempFolder, this.tracer);
                FileSystemExtensions.CleanupTempFolders(TempSubFolderName, tracer: this.tracer);
            }
        }

        /// <summary>
        /// Runs the child process task. This method reads and validates the command line
        /// arguments, starts listening to the parent process (for cancellation/termination),
        /// runs the specified function, and returns the result to the parent process.
        /// Should be called by the child process when it starts.
        /// </summary>
        /// <typeparam name="TInput">The child process input type</typeparam>
        /// <typeparam name="TOutput">The child process output type</typeparam>
        /// <param name="args">The command line arguments</param>
        /// <param name="functionToRun">The function to run</param>
        /// <param name="exceptionToExitCodeConverter">A function used to convert an exception thrown by the process to the exit code to return to the parent.</param>
        /// <param name="waitAfterFlush">Whether to wait after flushing the telemetry, to allow all traces to be sent.</param>
        /// <exception cref="ArgumentException">The wrong number of arguments was provided</exception>
        /// <returns>A <see cref="Task"/>, running the specified function and listening to the parent, returning the exit code to be returned from the process</returns>
        public async Task<int> RunAndListenToParentAsync<TInput, TOutput>(
            string[] args,
            Func<TInput, CancellationToken, Task<TOutput>> functionToRun,
            Func<Exception, int> exceptionToExitCodeConverter,
            bool waitAfterFlush = true)
            where TOutput : class
        {
            ChildProcessArguments arguments = ChildProcessArguments.FromCommandLineArguments(args);
            string pipeParentToChildHandle = arguments.PipeParentToChildHandle;
            string pipeChildToParentHandle = arguments.PipeChildToParentHandle;

            try
            {
                using (PipeStream pipe = new AnonymousPipeClientStream(PipeDirection.In, pipeParentToChildHandle))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    try
                    {
                        // Read the input
                        var input = await this.ReadFromStream<TInput>(pipe, cancellationTokenSource.Token);

                        // Start listening to parent process - run the listeners in separate tasks
                        // We should not wait on these tasks, since:
                        // * If any of these tasks fail, it will requests cancellation, and it is enough to wait on the main method and
                        //   let it handle cancellation gracefully
                        // * The cancellation listener is blocking, and cannot be canceled (anonymous pipes do not support cancellation).
                        //   Waiting on it will block the current thread.
#pragma warning disable 4014
                        this.ParentLiveListenerAsync(pipe, cancellationTokenSource);
                        this.ParentCancellationListenerAsync(pipe, cancellationTokenSource);
#pragma warning restore 4014

                        // Run the main function
                        TOutput output = await functionToRun(input, cancellationTokenSource.Token);

                        // Write the output back to the parent
                        await this.WriteChildProcessResult(pipeChildToParentHandle, output);

                        // Success - return zero for exit code
                        return 0;
                    }
                    catch (Exception e)
                    {
                        // If the exception is due to cancellation, than return dedicated error codes
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            await this.WriteChildProcessResult(pipeChildToParentHandle, "Child process was canceled by the parent");
                            return (int)HttpStatusCode.InternalServerError;
                        }

                        return await this.HandleChildProcessException(e, pipeChildToParentHandle, exceptionToExitCodeConverter);
                    }
                    finally
                    {
                        // Cancel the token to stop the listener tasks
                        cancellationTokenSource.Cancel();
                    }
                }
            }
            catch (Exception e)
            {
                return await this.HandleChildProcessException(e, pipeChildToParentHandle, exceptionToExitCodeConverter);
            }
            finally
            {
                this.tracer.Flush();
                if (waitAfterFlush)
                {
                    await Task.Delay(1000 * 5);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Write the input object to the specified stream
        /// </summary>
        /// <typeparam name="T">The type of object to write</typeparam>
        /// <param name="obj">The object to write</param>
        /// <param name="stream">The stream to write to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/>, writing the object to the stream</returns>
        private async Task WriteToStream<T>(T obj, Stream stream, CancellationToken cancellationToken)
        {
            // Note: we cannot use JsonWriter here, since it does not support async write with cancellation

            // Serialize the data
            byte[] inputBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Settings));
            this.tracer.TraceInformation($"Writing {inputBytes.Length} bytes to stream");

            // Write the number of bytes
            await stream.WriteAsync(BitConverter.GetBytes(inputBytes.Length), 0, sizeof(int), cancellationToken);

            // Write the bytes
            await stream.WriteAsync(inputBytes, 0, inputBytes.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Reads the input object from the stream
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <param name="stream">The stream to read from</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, reading the object from the stream and returning it</returns>
        private async Task<T> ReadFromStream<T>(Stream stream, CancellationToken cancellationToken)
        {
            // Note: we cannot use JsonReader here, since it does not support async read with cancellation

            // Read the number of bytes
            byte[] buffer = new byte[sizeof(int)];
            await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            int inputLength = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[inputLength];
            this.tracer.TraceInformation($"Reading {inputLength} bytes from stream");

            // Read the bytes
            int read = 0;
            while (read < buffer.Length)
            {
                int chunk = await stream.ReadAsync(buffer, read, buffer.Length - read, cancellationToken);
                read += chunk;
            }

            // Deserialize the data
            string bufferAsString = Encoding.UTF8.GetString(buffer);
            this.tracer.TraceVerbose($"Read object of type {typeof(T)} from stream: {bufferAsString}");
            return JsonConvert.DeserializeObject<T>(bufferAsString, Settings);
        }

        /// <summary>
        /// Write the child process result to the pipe
        /// </summary>
        /// <typeparam name="TOutput">The output type</typeparam>
        /// <param name="pipeChildToParentHandle">The pipe name</param>
        /// <param name="output">The output</param>
        /// <returns>A <see cref="Task"/>, writing the result to the pipe</returns>
        private async Task WriteChildProcessResult<TOutput>(string pipeChildToParentHandle, TOutput output)
        {
            using (PipeStream pipe = new AnonymousPipeClientStream(PipeDirection.Out, pipeChildToParentHandle))
            {
                if (pipe.IsConnected)
                {
                    // When writing process output, we do not support cancellation since we always want to write the output to the stream
                    await this.WriteToStream(output, pipe, default(CancellationToken));
                }
            }
        }

        /// <summary>
        /// Handles an exception thrown in the child process.
        /// </summary>
        /// <param name="e">The exception thrown.</param>
        /// <param name="pipeChildToParentHandle">The pipe handle, used to send the result to the parent process.</param>
        /// <param name="exceptionToExitCodeConverter">A function used to convert an exception thrown by the process to the exit code to return to the parent.</param>
        /// <returns>A <see cref="Task"/> running the current operation, returning the exit code to be returned by the process.</returns>
        private async Task<int> HandleChildProcessException(
            Exception e,
            string pipeChildToParentHandle,
            Func<Exception, int> exceptionToExitCodeConverter)
        {
            // Flatten an AggregateException to make it easier to process, and simplify if possible
            if (e is AggregateException aggregateException)
            {
                aggregateException = aggregateException.Flatten();
                if (aggregateException.InnerExceptions.Count == 1)
                {
                    e = aggregateException.InnerExceptions.Single();
                }
            }

            // Trace and write the result
            this.tracer.ReportException(e);
            this.tracer.TraceError($"Exception in child process: {e?.Message}");
            this.tracer.TraceVerbose($"Child process exception details: {e}");

            int exitCode = exceptionToExitCodeConverter(e);
            await this.WriteChildProcessResult(pipeChildToParentHandle, e.Message);
            return exitCode;
        }

        /// <summary>
        /// Checks if the parent process terminated, and if so, terminates the current process
        /// </summary>
        /// <param name="pipe">The pipe</param>
        /// <param name="cancellationTokenSource">The cancellation token Source</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        private async Task ParentLiveListenerAsync(PipeStream pipe, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                int notConnectedCount = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    // If the pipe is not connected, it means that either:
                    // 1. The parent process is dead.
                    // 2. The child process completed, but we have a race condition
                    //    and the cancellation request was not received yet.
                    // If the parent process is dead, we need to kill the child process.
                    // We check that the pipe is not connected for a long period of time,
                    // to avoid killing the process unnecessarily in case of a race condition.
                    if (!pipe.IsConnected)
                    {
                        notConnectedCount++;
                    }

                    if (notConnectedCount >= 20)
                    {
                        // Parent process terminated - terminate the child
                        string message = "Terminating the child process because the parent process was terminated";
                        this.tracer.TraceError(message);
                        Environment.FailFast(message);
                    }

                    await Task.Delay(100, cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // The task was canceled - no need to do anything, that's the normal flow
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Parent live listener threw an exception: {e}");

                // Something bad happened - kill the process
                Environment.FailFast("Terminating the child process because the parent live listener threw an exception: {e}");
            }
            finally
            {
                this.tracer.TraceInformation("Parent live listener completed");
            }
        }

        /// <summary>
        /// Listens to the parent process for cancellation
        /// </summary>
        /// <param name="pipe">The pipe</param>
        /// <param name="cancellationTokenSource">The cancellation token source</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        private async Task ParentCancellationListenerAsync(PipeStream pipe, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    // Read bytes from the pipe
                    string s = await this.ReadFromStream<string>(pipe, cancellationTokenSource.Token);

                    // Check if we got a cancellation instruction
                    if (s == CancellationString)
                    {
                        this.tracer.TraceInformation("Cancellation instruction received from parent - canceling");
                        cancellationTokenSource.Cancel();
                        break;
                    }

                    await Task.Delay(100, cancellationTokenSource.Token);
                }
            }
            catch (ObjectDisposedException)
            {
                // The pipe was disposed - meaning the process is completed, ignore
            }
            catch (TaskCanceledException)
            {
                // The task was canceled - no need to do anything, that's the normal flow
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Parent cancellation listener threw an exception: {e}");

                // Something bad happened - kill the process
                Environment.FailFast("Terminating the child process because the parent live listener threw an exception: {e}");
            }
            finally
            {
                this.tracer.TraceInformation("Parent cancellation listener completed");
            }
        }

        /// <summary>
        /// Sends a cancellation instruction to the child process, and ensures that it exits
        /// </summary>
        /// <param name="childProcess">The child process</param>
        /// <param name="pipe">The pipe</param>
        /// <param name="wasChildTerminatedByParent">A flag indicating whether the child process did not gracefully exit in the designated period of time, and was terminated</param>
        private void CancelChildProcess(Process childProcess, PipeStream pipe, ref bool wasChildTerminatedByParent)
        {
            // Send a cancellation instruction down the pipe
            this.WriteToStream(CancellationString, pipe, default(CancellationToken)).Wait();
            this.tracer.TraceInformation($"Cancellation instruction sent to child process, process ID {childProcess.Id}");

            // Give the process some time to gracefully exit (by default 4 minutes) - if it doesn't exit, kill it
            bool processExited = childProcess.WaitForExit((int)TimeSpan.FromSeconds(this.CancellationGraceTimeInSeconds).TotalMilliseconds);
            if (processExited)
            {
                this.tracer.TraceInformation($"The child process exited, process ID {childProcess.Id}");
            }
            else
            {
                this.tracer.TraceInformation($"The child process did not terminate in time - killing the process, process ID {childProcess.Id}");
                childProcess.Kill();
                wasChildTerminatedByParent = true;
            }
        }

        #endregion

        #region Internal definitions

        /// <summary>
        /// This class manages the child process command line argument
        /// </summary>
        private class ChildProcessArguments
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ChildProcessArguments"/> class
            /// </summary>
            /// <param name="pipeParentToChildHandle">The handle to the pipe from the parent to the child</param>
            /// <param name="pipeChildToParentHandle">The handle to the pipe from the child to the parent</param>
            /// <param name="sessionId">The session Id</param>
            /// <param name="customTracerProperties">The custom tracer properties</param>
            /// <param name="tempFolder">The temporary folder path</param>
            public ChildProcessArguments(string pipeParentToChildHandle, string pipeChildToParentHandle, string sessionId, IReadOnlyDictionary<string, string> customTracerProperties, string tempFolder)
            {
                this.PipeParentToChildHandle = pipeParentToChildHandle;
                this.PipeChildToParentHandle = pipeChildToParentHandle;
                this.SessionId = sessionId;
                this.CustomTracerProperties = customTracerProperties;
                this.TempFolder = tempFolder;
            }

            /// <summary>
            /// Gets the handle to the pipe from the parent to the child
            /// </summary>
            public string PipeParentToChildHandle { get; }

            /// <summary>
            /// Gets the handle to the pipe from the child to the parent
            /// </summary>
            public string PipeChildToParentHandle { get; }

            /// <summary>
            /// Gets the session Id
            /// </summary>
            public string SessionId { get; }

            /// <summary>
            /// Gets the custom tracer properties
            /// </summary>
            public IReadOnlyDictionary<string, string> CustomTracerProperties { get; }

            /// <summary>
            /// Gets the temporary folder path
            /// </summary>
            public string TempFolder { get; }

            /// <summary>
            /// Converts the specified instance of <see cref="ChildProcessArguments"/> to command line arguments
            /// </summary>
            /// <param name="arguments">The <see cref="ChildProcessArguments"/> instance</param>
            /// <returns>The command line arguments (as string)</returns>
            public static string ToCommandLineArguments(ChildProcessArguments arguments)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arguments, Formatting.None)));
            }

            /// <summary>
            /// Converts the specified command line arguments to an instance of <see cref="ChildProcessArguments"/>
            /// </summary>
            /// <param name="args">The command line arguments</param>
            /// <returns>The <see cref="ChildProcessArguments"/> instance</returns>
            public static ChildProcessArguments FromCommandLineArguments(string[] args)
            {
                if (args == null || args.Length != 1)
                {
                    throw new ArgumentException($"Invalid number of command line arguments - expected 1, actual {args?.Length}", nameof(args));
                }

                return JsonConvert.DeserializeObject<ChildProcessArguments>(Encoding.UTF8.GetString(Convert.FromBase64String(args[0])));
            }
        }

        #endregion
    }
}