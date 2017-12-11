namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for a factory class that creates instances of <see cref="ISmartSignalAnalysisServices"/>
    /// </summary>
    public interface ISmartSignalAnalysisServicesFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ISmartSignalAnalysisServices"/>, that
        /// provides analysis services for the specified resources.
        /// </summary>
        /// <param name="resources">The resources to analyze</param>
        /// <returns>An instances of <see cref="ISmartSignalAnalysisServices"/> for these resources</returns>
        Task<ISmartSignalAnalysisServices> CreateAsync(IList<ResourceIdentifier> resources);
    }
}