//-----------------------------------------------------------------------
// <copyright file="MsiCredentials.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using Polly;

    /// <summary>
    /// A class that represents managed service identity credentials
    /// </summary>
    public class MsiCredentials : ServiceClientCredentials
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "MSI";

        /// <summary>
        /// The token expiry time, in minutes
        /// </summary>
        private const int TokenExpiryInMinutes = 10;

        private static readonly SemaphoreSlim TokensCacheSemaphore = new SemaphoreSlim(1, 1);
        private static readonly MemoryCache TokensCache = new MemoryCache("MsiTokens");

        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly string resource;
        private readonly Policy<HttpResponseMessage> retryPolicy;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsiCredentials"/> class.
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="resource">The resource name - the URL for which these credentials will be used</param>
        /// <param name="tracer">The tracer</param>
        public MsiCredentials(IHttpClientWrapper httpClientWrapper, string resource, ITracer tracer)
        {
            this.resource = resource;
            this.httpClientWrapper = httpClientWrapper;
            this.tracer = tracer;
            this.retryPolicy = PolicyExtensions.CreateTransientHttpErrorPolicy(this.tracer, DependencyName);
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
            string token = (string)TokensCache.Get(this.resource);
            if (token == null)
            {
                // Use a semaphore to lock (can't use "await" inside a "lock")
                await TokensCacheSemaphore.WaitAsync(cancellationToken);
                try
                {
                    token = (string)TokensCache.Get(this.resource);
                    if (token == null)
                    {
                        token = await this.GetTokenAsync(cancellationToken);
                        TokensCache.Set(this.resource, token, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(TokenExpiryInMinutes) });
                    }
                }
                finally
                {
                    TokensCacheSemaphore.Release();
                }
            }

            // Add the authentication header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            HttpResponseMessage response = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                string.Empty,
                () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/?resource={HttpUtility.UrlEncode(this.resource)}&api-version=2017-09-01");
                    request.Headers.Add("Secret", secret);

                    return this.httpClientWrapper.SendAsync(request, null, cancellationToken);
                });

            if (!response.IsSuccessStatusCode)
            {
                string content = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                var message = $"Failed to get MSI token. Fail StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}. Content: {content}.";
                this.tracer.TraceError(message);
                throw new MsiTokenException(message);
            }

            // Extract the token from the response
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseObject.access_token;
        }
    }
}