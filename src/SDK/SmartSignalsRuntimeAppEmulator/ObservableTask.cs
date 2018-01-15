//-----------------------------------------------------------------------
// <copyright file="ObservableTask.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An observable task, inherits from <see cref="ObservableObject"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result being returned from the task. This type should have default constructor, 
    /// so it can be used as the task result as long as the task has not finished running</typeparam>
    public class ObservableTask<TResult> : ObservableObject
    {
        private Task<TResult> task;

        private TResult result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableTask{TResult}"/> class.
        /// </summary>
        /// <param name="task">The task that should be observed</param>
        public ObservableTask(Task<TResult> task)
        {
            this.Task = task;
            this.Result = default(TResult);
            if (!task.IsCompleted)
            {
                var taskResult = this.RunTaskAsync();
            }
        }

        /// <summary>
        /// Gets the observed task.
        /// </summary>
        public Task<TResult> Task
        {
            get
            {
                return this.task;
            }

            private set
            {
                this.task = value;
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
        /// Runs the task asynchronously.
        /// </summary>
        /// <returns>The observed task</returns>
        private async Task RunTaskAsync()
        {
            try
            {
                await this.Task;
            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            this.Result = this.Task.Status == TaskStatus.RanToCompletion ?
                this.Task.Result :
                default(TResult);
        }
    }
}
