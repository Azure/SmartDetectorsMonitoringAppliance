//-----------------------------------------------------------------------
// <copyright file="BlobStateRepositoryFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.State
{
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// Represents a factory for creating blob state repository for a Smart Detector.
    /// </summary>
    public class BlobStateRepositoryFactory : IStateRepositoryFactory
    {
        private readonly ICloudStorageProviderFactory cloudStorageProviderFactory;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStateRepositoryFactory"/> class
        /// </summary>
        /// <param name="cloudStorageProviderFactory">The cloud storage provider factory</param>
        /// <param name="tracer">The tracer</param>
        public BlobStateRepositoryFactory(ICloudStorageProviderFactory cloudStorageProviderFactory, ITracer tracer)
        {
            this.cloudStorageProviderFactory = Diagnostics.EnsureArgumentNotNull(() => cloudStorageProviderFactory);
            this.tracer = tracer;
        }

        /// <summary>
        /// Creates a state repository for a Smart Detector with ID <paramref name="smartDetectorId"/>.
        /// </summary>
        /// <param name="smartDetectorId">The ID of the Smart Detector to create the state repository for.</param>
        /// <param name="alertRuleResourceId">The resource ID of the Alert Rule to create the state repository for.</param>
        /// <returns>A state repository associated with the requested Smart Detector.</returns>
        public IStateRepository Create(string smartDetectorId, string alertRuleResourceId)
        {
            return new BlobStateRepository(smartDetectorId, alertRuleResourceId, this.cloudStorageProviderFactory, this.tracer);
        }
    }
}
