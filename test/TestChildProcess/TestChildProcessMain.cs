//-----------------------------------------------------------------------
// <copyright file="TestChildProcessMain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestChildProcess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ChildProcess;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Moq;

    public static class TestChildProcessMain
    {
        public static int Main(string[] args)
        {
            var tracerMock = new Mock<IExtendedTracer>();
            IChildProcessManager childProcessManager = new ChildProcessManager(tracerMock.Object);
            return childProcessManager.RunAndListenToParentAsync<TestChildProcessInput, TestChildProcessOutput>(args, MainTask, ConvertExceptionToExitCode, false).GetAwaiter().GetResult();
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
                    throw new SmartDetectorRepositoryException("abc");
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

        private static int ConvertExceptionToExitCode(Exception e)
        {
            if (e is SmartDetectorRepositoryException)
            {
                return 1948;
            }

            return -1;
        }
    }
}
