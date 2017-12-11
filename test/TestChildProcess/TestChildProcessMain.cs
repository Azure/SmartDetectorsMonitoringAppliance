namespace TestChildProcess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess;
    using Moq;

    public static class TestChildProcessMain
    {
        public static void Main(string[] args)
        {
            var tracerMock = new Mock<ITracer>();
            IChildProcessManager childProcessManager = new ChildProcessManager(tracerMock.Object);
            childProcessManager.RunAndListenToParentAsync<TestChildProcessInput, TestChildProcessOutput>(args, MainTask).Wait();
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
                    throw new DivideByZeroException();
                case RunMode.Cancellation:
                    await Task.Delay(int.MaxValue, cancellationToken);
                    break;
                case RunMode.Stuck:
                    await Task.Delay(int.MaxValue, default(CancellationToken));
                    break;
            }

            return null;
        }
    }
}
