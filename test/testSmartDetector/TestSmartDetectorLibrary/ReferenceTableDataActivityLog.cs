//-----------------------------------------------------------------------
// <copyright file="ReferenceTableDataActivityLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TestSmartDetectorLibrary
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Newtonsoft.Json;

    public class ReferenceTableDataActivityLog
    {
        [JsonProperty("operationId")]
        [TableColumn("Operation Id", "operationId", Order = 1)]
        public string OperationId { get; set; }

        [JsonProperty("level")]
        [TableColumn("Level", "level", Order = 1)]
        public string Level { get; set; }

        [JsonProperty("eventTimestamp")]
        [TableColumn("operation time", "eventTimestamp", Order = 1)]
        public string EventTimestamp { get; set; }
    }
}