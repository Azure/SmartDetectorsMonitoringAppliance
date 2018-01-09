//-----------------------------------------------------------------------
// <copyright file="HttpClientWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An class that implements the <see cref="IHttpClientWrapper"/> interface.
    /// We are wrapping the HttpClient class in order to make it testable.
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWrapper"/> class.
        /// </summary>
        public HttpClientWrapper()
        {
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Gets or sets the request timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return this.httpClient.Timeout; }
            set { this.httpClient.Timeout = value; }
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await this.httpClient.SendAsync(request, cancellationToken);
        }
    }
}