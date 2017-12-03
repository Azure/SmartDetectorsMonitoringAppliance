namespace Microsoft.SmartSignals.Scheduler
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class Schedule
    {
        [FunctionName("Schedule")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // read config

            // pass config to signal tracker

            // get resources

            // for each signal send to analysis

            // save detections in AI
        }
    }
}
