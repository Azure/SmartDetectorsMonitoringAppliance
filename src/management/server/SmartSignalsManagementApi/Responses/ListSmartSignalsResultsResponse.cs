//-----------------------------------------------------------------------
// <copyright file="ListSmartSignalsResultsResponse.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses
{
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the GET Management API operation for listing signals results.
    /// </summary>
    public class ListSmartSignalsResultsResponse
    {
        /// <summary>
        /// Gets or sets the smart signals results list
        /// </summary>
        [JsonProperty("signalsResults")]
        public IList<SmartSignalResultItemPresentation> SignalsResults { get; set; }
    }
}
