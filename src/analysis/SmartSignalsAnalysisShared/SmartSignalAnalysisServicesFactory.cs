namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the <see cref="ISmartSignalAnalysisServicesFactory"/> interface
    /// </summary>
    public class SmartSignalAnalysisServicesFactory : ISmartSignalAnalysisServicesFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ISmartSignalAnalysisServices"/>, that
        /// provides analysis services for the specified resources.
        /// </summary>
        /// <param name="resources">The resources to analyze</param>
        /// <returns>An instances of <see cref="ISmartSignalAnalysisServices"/> for these resources</returns>
        public Task<ISmartSignalAnalysisServices> CreateAsync(IList<ResourceIdentifier> resources)
        {
            throw new System.NotImplementedException();
        }
    }
}