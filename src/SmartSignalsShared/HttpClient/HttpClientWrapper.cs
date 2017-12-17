namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.HttpClient
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An class that implements the <see cref="IHttpClientWrapper"/> interface.
    /// We are wrapping the HttpClient class in order to make it mockable and testable.
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWrapper"/> class.
        /// </summary>
        /// <param name="timeout">(optional) the request timeout.</param>
        public HttpClientWrapper(TimeSpan? timeout = null)
        {
            this.httpClient = new HttpClient();

            if (timeout != null)
            {
                this.httpClient.Timeout = timeout.Value;
            }
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.httpClient.SendAsync(request, cancellationToken);
        }
    }
}
