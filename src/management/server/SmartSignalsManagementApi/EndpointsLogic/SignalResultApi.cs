//-----------------------------------------------------------------------
// <copyright file="SignalResultApi.cs" company="Microsoft Corporation">
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
    public class SignalResultApi : ISignalResultApi
    {
        /// <summary>
        /// Gets all the detections.
        /// </summary>
        /// <returns>The smart detections.</returns>
        public Task<IEnumerable<SmartSignalDetection>> GetAllDetectionsAsync()
        {
            return null;
        }
    }
}
