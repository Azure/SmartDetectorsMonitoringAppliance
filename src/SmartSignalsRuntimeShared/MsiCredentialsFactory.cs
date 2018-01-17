//-----------------------------------------------------------------------
// <copyright file="MsiCredentialsFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared
{
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.HttpClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Rest;

    /// <summary>
    /// An implementation of the <see cref="ICredentialsFactory"/>, creating MSI credentials.
    /// </summary>
    public class MsiCredentialsFactory : ICredentialsFactory
    {
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiCredentialsFactory"/> class
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="tracer">The tracer</param>
        public MsiCredentialsFactory(IHttpClientWrapper httpClientWrapper, ITracer tracer)
        {
            this.httpClientWrapper = httpClientWrapper;
            this.tracer = tracer;
        }

        /// <summary>
        /// Create an instance of the <see cref="ServiceClientCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public ServiceClientCredentials Create(string resource)
        {
            return new MsiCredentials(this.httpClientWrapper, resource, this.tracer);
        }
    }
}