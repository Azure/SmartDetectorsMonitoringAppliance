//-----------------------------------------------------------------------
// <copyright file="ObservableTask.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// An Observable Task with no return value.
    /// </summary>
    public class ObservableTask : ObservableTask<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableTask"/> class.
        /// </summary>
        /// <param name="taskToRun">The task that should be observed.</param>
        /// <param name="tracer">The tracer.</param>
        /// <param name="onTaskCompletedCallbabk">Method that should be executed after the task was completed.</param>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "This is a misfire of the rule, as it shouldn't be applied to constructors (see issue #1691)")]
        public ObservableTask(Task taskToRun, ITracer tracer, OnTaskCompletedEventHandler<bool> onTaskCompletedCallbabk = null)
            : base(TaskToRunWrapper(taskToRun), tracer, onTaskCompletedCallbabk)
        {
        }

        /// <summary>
        /// A wrapper for converting a <see cref="Task"/> with no return value to one returning a dummy boolean result.
        /// </summary>
        /// <param name="taskToRun">The actual task to run.</param>
        /// <returns>The same task, just returning a value.</returns>
        private static async Task<bool> TaskToRunWrapper(Task taskToRun)
        {
            await taskToRun;
            return true;
        }
    }
}
