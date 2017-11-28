namespace Microsoft.SmartSignals.Scheduler
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class Schedule
    {
        [FunctionName("Schedule")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            
        }
    }
}
