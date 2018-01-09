//-----------------------------------------------------------------------
// <copyright file="SignalApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Models;
    using Responses;
    using Shared;

    /// <summary>
    /// This class is the logic for the /signal endpoint.
    /// </summary>
    public class SignalApi : ISignalApi
    {
        private readonly ISmartSignalRepository smartSignalsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalApi"/> class.
        /// </summary>
        /// <param name="smartSignalsRepository">The smart signal repository.</param>
        public SignalApi(ISmartSignalRepository smartSignalsRepository)
        {
            Diagnostics.EnsureArgumentNotNull(() => smartSignalsRepository);

            this.smartSignalsRepository = smartSignalsRepository;
        }

        /// <summary>
        /// List all the smart signals.
        /// </summary>
        /// <returns>The smart signals.</returns>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to retrieve smart signals.</exception>
        public async Task<ListSmartSignalsResponse> GetAllSmartSignalsAsync()
        {
            try
            {
                IList<SmartSignalManifest> smartSignalManifests = await this.smartSignalsRepository.ReadAllSignalsManifestsAsync();

                // Convert smart signals to the required response
                var signals = smartSignalManifests.Select(manifest => new Signal
                {
                   Id = manifest.Id,
                   Name = manifest.Name,
                   SupportedCadences = new List<int>(manifest.SupportedCadencesInMinutes),
                   Configurations = new List<SignalConfiguration>()
                }).ToList();

                return new ListSmartSignalsResponse()
                {
                    Signals = signals
                };
            }
            catch (Exception e) 
            {
                throw new SmartSignalsManagementApiException("Failed to get smart signals", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}
