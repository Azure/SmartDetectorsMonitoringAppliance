//-----------------------------------------------------------------------
// <copyright file="ObservableTask.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Occurs when the observable task was completed.
    /// </summary>
    /// <typeparam name="T">The type of the object that will be passed as an input parameter.</typeparam>
    /// <param name="obj">The input parameter.</param>
    public delegate void OnTaskCompletedEventHandler<T>(T obj);

    /// <summary>
    /// An observable task, inherits from <see cref="ObservableObject"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result being returned from the task. This type should have default constructor,
    /// so it can be used as the task result as long as the task has not finished running</typeparam>
    public class ObservableTask<TResult> : ObservableObject
    {
        private readonly ITracer tracer;

        private Task<TResult> taskToRun;

        private TResult result;

        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableTask{TResult}"/> class.
        /// </summary>
        /// <param name="taskToRun">The task that should be observed.</param>
        /// <param name="tracer">The tracer.</param>
        /// <param name="onTaskCompletedCallbabk">Method that should be executed after the task was completed.</param>
        public ObservableTask(Task<TResult> taskToRun, ITracer tracer, OnTaskCompletedEventHandler<TResult> onTaskCompletedCallbabk = null)
        {
            this.TaskToRun = taskToRun;
            this.tracer = tracer;

            if (onTaskCompletedCallbabk != null)
            {
                this.OnTaskCompleted += onTaskCompletedCallbabk;
            }

            this.Result = default(TResult);
            this.IsRunning = taskToRun.Status == TaskStatus.Running;

            if (!taskToRun.IsCompleted)
            {
                #pragma warning disable 4014
                this.RunTaskAsync();
                #pragma warning restore 4014
            }
            else
            {
                this.Result = this.GetTaskResult();
                this.OnTaskCompleted?.Invoke(this.Result);
            }
        }

        /// <summary>
        /// Handler for actions that should be done after the task was completed.
        /// </summary>
        public event OnTaskCompletedEventHandler<TResult> OnTaskCompleted;

        /// <summary>
        /// Gets the observed task.
        /// </summary>
        public Task<TResult> TaskToRun
        {
            get
            {
                return this.taskToRun;
            }

            private set
            {
                this.taskToRun = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the observed task's result.
        /// </summary>
        public TResult Result
        {
            get
            {
                return this.result;
            }

            private set
            {
                this.result = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the task is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }

            private set
            {
                this.isRunning = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Runs the task asynchronously.
        /// </summary>
        /// <returns>The observed task</returns>
        private async Task RunTaskAsync()
        {
            this.IsRunning = true;

            try
            {
                await this.TaskToRun;
            }
            catch (Exception e)
            {
                this.tracer.TraceError($"Task failed with exception: {e}");
            }
            finally
            {
                this.IsRunning = false;

                this.Result = this.GetTaskResult();

                this.OnTaskCompleted?.Invoke(this.Result);
            }
        }

        /// <summary>
        /// Gets the task result.
        /// </summary>
        /// <returns>The task result or default value of <typeparamref name="TResult"/> in case task didn't complete.</returns>
        private TResult GetTaskResult()
        {
            return this.TaskToRun.Status == TaskStatus.RanToCompletion ?
                this.TaskToRun.Result :
                default(TResult);
        }
    }
}
