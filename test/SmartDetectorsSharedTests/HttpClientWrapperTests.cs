//-----------------------------------------------------------------------
// <copyright file="HttpClientWrapperTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HttpClientWrapperTests
    {
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task WhenCallingSendAsyncThenTimeoutIsRespected()
        {
            IHttpClientWrapper httpClient = new HttpClientWrapper();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://portal.azure.com"));
            await httpClient.SendAsync(request, TimeSpan.FromMilliseconds(5), CancellationToken.None);
        }
    }
}