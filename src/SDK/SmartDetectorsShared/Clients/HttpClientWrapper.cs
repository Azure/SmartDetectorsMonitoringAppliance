//-----------------------------------------------------------------------
// <copyright file="HttpClientWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
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
        // Use a single static instance of HttpClient, with large timeout - the
        // actual timeout wil be set for each request in the call to SendAsync
        private static readonly HttpClient HttpClient = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan };

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="timeout">The request timeout, or null to use the default timeout of 5 minutes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                timeoutCancellationTokenSource.CancelAfter(timeout ?? TimeSpan.FromSeconds(100));
                using (CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token))
                {
                    try
                    {
                        return await HttpClient.SendAsync(request, linkedCancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException e)
                    {
                        // HttpClient will throw TaskCanceledException instead of timeout exception,
                        // so convert the exception if the request's token is canceled but the outer token is not
                        if (timeoutCancellationTokenSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                        {
                            throw new TimeoutException("HTTP request timed out", e);
                        }

                        throw;
                    }
                }
            }
        }
    }
}