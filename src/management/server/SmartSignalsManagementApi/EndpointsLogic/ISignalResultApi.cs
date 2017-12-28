//-----------------------------------------------------------------------
// <copyright file="ISignalResultApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This class is the logic for the /signalResult endpoint.
    /// </summary>
    public interface ISignalResultApi
    {
        /// <summary>
        /// Gets all the Smart Signal results.
        /// </summary>
        /// <returns>The Smart Signal results.</returns>
        Task<IEnumerable<SmartSignalResult>> GetAllSmartSignalResultsAsync();
    }
}
