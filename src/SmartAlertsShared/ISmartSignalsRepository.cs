namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
{
    using System.Threading.Tasks;

    public interface ISmartSignalsRepository
    {
        Task<SmartSignalMetadata> ReadSignalMetadataAsync(string signalId);
    }
}