//-----------------------------------------------------------------------
// <copyright file="MsiCredentialsFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance
{
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.Rest;

    /// <summary>
    /// An implementation of the <see cref="ICredentialsFactory"/>, creating MSI credentials.
    /// </summary>
    public class MsiCredentialsFactory : ICredentialsFactory
    {
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly IExtendedTracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiCredentialsFactory"/> class
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="tracer">The tracer</param>
        public MsiCredentialsFactory(IHttpClientWrapper httpClientWrapper, IExtendedTracer tracer)
        {
            this.httpClientWrapper = httpClientWrapper;
            this.tracer = tracer;
        }

        /// <summary>
        /// Create an instance of the <see cref="ServiceClientCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public ServiceClientCredentials CreateServiceClientCredentials(string resource)
        {
            return new MsiCredentials(this.httpClientWrapper, resource, this.tracer);
        }

        /// <summary>
        /// Create an instance of the <see cref="AzureCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public AzureCredentials CreateAzureCredentials(string resource)
        {
            var msiCredentials = new MsiCredentials(this.httpClientWrapper, resource, this.tracer);

            return new AzureCredentials(
                armCredentials: msiCredentials,
                graphCredentials: msiCredentials,
                tenantId: "microsoft.com",
                environment: AzureEnvironment.AzureGlobalCloud);
        }
    }
}