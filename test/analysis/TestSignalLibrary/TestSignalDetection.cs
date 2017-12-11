namespace TestSignalLibrary
{
    using Microsoft.Azure.Monitoring.SmartSignals;

    public class TestSignalDetection : SmartSignalDetection
    {
        public TestSignalDetection(string title)
        {
            this.Title = title;
        }

        public override string Title { get; }
    }
}