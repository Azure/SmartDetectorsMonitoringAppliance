namespace TestChildProcess
{
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
}