//-----------------------------------------------------------------------
// <copyright file="ActiveDirectoryCredentials.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;

    /// <summary>
    /// A class that represents Azure Active Directory identity credentials.
    /// </summary>
    public class ActiveDirectoryCredentials : ServiceClientCredentials
    {
        private readonly IAuthenticationServices authenticationServices;

        private readonly string resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryCredentials"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services</param>
        /// <param name="resource">The resource for which to create the credentials</param>
        public ActiveDirectoryCredentials(IAuthenticationServices authenticationServices, string resource)
        {
            this.authenticationServices = authenticationServices;
            this.resource = resource;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request and process it.
        /// </summary>
        /// <param name="request">The HTTP request message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that will complete when processing has finished</returns>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add the authentication header
            string resourceToken = await this.authenticationServices.GetResourceTokenAsync(this.resource);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resourceToken);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
