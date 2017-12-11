namespace TestChildProcess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.ChildProcess;
    using Moq;

    /// <summary>
    /// This enum is used to specify the expected behavior of the
    /// child process, to test various test scenarios.
    /// </summary>
    public enum RunMode
    {
        Happy,
        Null,
        Exception,
        Cancellation,
        Stuck,
    }

    /// <summary>
    /// The child process input type
    /// </summary>
    public class TestChildProcessInput
    {
        public const int ExpectedIntValue = 2347;

        public const string ExpectedStringValue = "Premature optimization";

        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public RunMode RunMode { get; set; }
    }

    /// <summary>
    /// The child process output type
    /// </summary>
    public class TestChildProcessOutput
    {
        public const int ExpectedIntValue = 70;

        public const string ExpectedStringValue = "is the root of all evil";

        public int IntValue { get; set; }

        public string StringValue { get; set; }
    }

    static class TestChildProcessMain
    {
        static void Main(string[] args)
        {
            var tracerMock = new Mock<ITracer>();
            IChildProcessManager childProcessManager = new ChildProcessManager();
            childProcessManager.RunAndListenToParentAsync<TestChildProcessInput, TestChildProcessOutput>(args, MainTask, tracerMock.Object).Wait();
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
