namespace Microsoft.SmartAlerts.Shared
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.SmartAlerts.Shared;

    public interface ISignalsRepository
    {
        Task<SignalMetadata> ReadSignalMetadataAsync(string signalId);
    }
}