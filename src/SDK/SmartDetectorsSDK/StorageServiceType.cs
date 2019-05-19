//-----------------------------------------------------------------------
// <copyright file="StorageServiceType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of all Azure storage services supported by Smart Detectors.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StorageServiceType
    {
        /// <summary>
        /// No specific service type
        /// </summary>
        None,

        /// <summary>
        /// Blob service
        /// </summary>
        Blob,

        /// <summary>
        /// Table service
        /// </summary>
        Table,

        /// <summary>
        /// Queue service
        /// </summary>
        Queue,

        /// <summary>
        /// File service
        /// </summary>
        File
    }
}
