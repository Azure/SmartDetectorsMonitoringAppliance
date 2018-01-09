//-----------------------------------------------------------------------
// <copyright file="ListSmartSignalsResponse.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses
{
    using System.Collections.Generic;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the GET Management API operation for listing signals.
    /// </summary>
    public class ListSmartSignalsResponse
    {
        /// <summary>
        /// Gets or sets the smart signals list
        /// </summary>
        [JsonProperty("signals")]
        public IList<Signal> Signals { get; set; }
    }
}
