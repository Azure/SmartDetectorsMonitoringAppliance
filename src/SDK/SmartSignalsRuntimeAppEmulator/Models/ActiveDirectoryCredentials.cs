//-----------------------------------------------------------------------
// <copyright file="ActiveDirectoryCredentials.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
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
        private readonly string token;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryCredentials"/> class.
        /// </summary>
        /// <param name="token">The authentication result token</param>
        public ActiveDirectoryCredentials(string token)
        {
            this.token = token;
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
