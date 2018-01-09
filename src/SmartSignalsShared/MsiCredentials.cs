//-----------------------------------------------------------------------
// <copyright file="MsiCredentials.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient;
    using Microsoft.Rest;
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represents managed service identity credentials
    /// </summary>
    public class MsiCredentials : ServiceClientCredentials
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly string resource;

        private string token;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiCredentials"/> class.
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="resource">The resource name - the URL for which these credentials will be used</param>
        public MsiCredentials(IHttpClientWrapper httpClientWrapper, string resource)
        {
            this.resource = resource;
            this.httpClientWrapper = httpClientWrapper;
            this.token = null;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task that will complete when processing has finished.</returns>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get the access token
            if (this.token == null)
            {
                // Use a semaphore to lock (can't use "await" inside a "lock")
                await this.semaphore.WaitAsync(cancellationToken);
                try
                {
                    if (this.token == null)
                    {
                        this.token = await this.GetTokenAsync(cancellationToken);
                    }
                }
                finally
                {
                    this.semaphore.Release();
                }
            }

            // Add the authentication header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Access the MSI endpoint to get the token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/>, returning the token.</returns>
        private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            // Get MSI parameters from environment variables
            string endpoint = Environment.GetEnvironmentVariable("MSI_ENDPOINT");
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ApplicationException("The MSI endpoint was not found in the application's environment variables");
            }

            string secret = Environment.GetEnvironmentVariable("MSI_SECRET");
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new ApplicationException("The MSI secret was not found in the application's environment variables");
            }

            // Call the MSI endpoint
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/?resource={HttpUtility.UrlEncode(this.resource)}&api-version=2017-09-01");
            request.Headers.Add("Secret", secret);
            var response = await this.httpClientWrapper.SendAsync(request, cancellationToken);

            // Extract the token from the response
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseObject.access_token;
        }
    }
}