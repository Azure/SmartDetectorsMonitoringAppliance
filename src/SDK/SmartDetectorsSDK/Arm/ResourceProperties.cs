//-----------------------------------------------------------------------
// <copyright file="ResourceProperties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Arm
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A class holding different properties of an ARM resource
    /// </summary>
    public class ResourceProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceProperties"/> class
        /// </summary>
        /// <param name="sku">The resource SKU object</param>
        /// <param name="properties">The resource properties</param>
        /// <param name="apiVersion">The API version that was used for the ARM call</param>
        public ResourceProperties(ResourceSku sku, JObject properties, string apiVersion)
        {
            this.Sku = sku;
            this.Properties = properties;
            this.ApiVersion = apiVersion;
        }

        /// <summary>
        /// Gets the resource SKU object
        /// </summary>
        public ResourceSku Sku { get; }

        /// <summary>
        /// Gets the resource properties
        /// </summary>
        public JObject Properties { get; }

        /// <summary>
        /// Gets the API version that was used for the ARM call.
        /// The version can be used by the caller to infer the structure of data in <see cref="Properties"/>.
        /// </summary>
        public string ApiVersion { get; }
    }
}