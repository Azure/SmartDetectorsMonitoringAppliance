//-----------------------------------------------------------------------
// <copyright file="TestChildProcessMain.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestChildProcess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.Exceptions;
    using Moq;

    public static class TestChildProcessMain
    {
        public static void Main(string[] args)
        {
            var tracerMock = new Mock<ITracer>();
            IChildProcessManager childProcessManager = new ChildProcessManager(tracerMock.Object);
            childProcessManager.RunAndListenToParentAsync<TestChildProcessInput, TestChildProcessOutput>(args, MainTask, false).Wait();
        }

        private static async Task<TestChildProcessOutput> MainTask(TestChildProcessInput input, CancellationToken cancellationToken)
        {
            // Verify we got the right input
            if (input.IntValue != TestChildProcessInput.ExpectedIntValue || input.StringValue != TestChildProcessInput.ExpectedStringValue)
            {
                throw new ArgumentException($"Unexpected input: InValue={input.IntValue}, StringValue={input.StringValue}");
            }

            // And perform the action based on the RunMode
            switch (input.RunMode)
            {
                case RunMode.Happy:
                    return new TestChildProcessOutput()
                    {
                        IntValue = TestChildProcessOutput.ExpectedIntValue,
                        StringValue = TestChildProcessOutput.ExpectedStringValue
                    };
                case RunMode.Null:
                    return null;
                case RunMode.Exception:
                    throw new SmartSignalRepositoryException("abc");
                case RunMode.Cancellation:
                    await Task.Delay(int.MaxValue, cancellationToken);
                    break;
                case RunMode.Stuck:
                    await Task.Delay(int.MaxValue, default(CancellationToken));
                    break;
                case RunMode.Crash:
                    await Task.Delay(100, default(CancellationToken));
                    Environment.FailFast(string.Empty);
                    break;
            }

            return null;
        }
    }
}
