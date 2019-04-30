//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerRequestTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests.AlertPresentation
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AzureResourceManagerRequestTests
    {
        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithUriThenRequestIsCreated()
        {
            var armRequest = new TestAzureResourceManagerRequest(new Uri("/some/request/path", UriKind.Relative));
            Assert.AreEqual("/some/request/path", armRequest.RequestUri.ToString());
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithNullUriThenExceptionIsThrown()
        {
            _ = new TestAzureResourceManagerRequest(null);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithAbsoluteUriThenExceptionIsThrown()
        {
            _ = new TestAzureResourceManagerRequest(new Uri("https://www.microsoft.com"));
        }

        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithResourceIdAndSuffixThenRequestIsCreated()
        {
            var resource = new ResourceIdentifier(ResourceType.AzureStorage, "subscription", "resourceGroup", "resourceName");

            var nullSuffixRequest = new TestAzureResourceManagerRequest(resource, null);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName", nullSuffixRequest.RequestUri.ToString(), "Wrong RequestUri for nullSuffixRequest");

            var emptySuffixRequest = new TestAzureResourceManagerRequest(resource, null);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName", emptySuffixRequest.RequestUri.ToString(), "Wrong RequestUri for emptySuffixRequest");

            var pathSuffixRequest = new TestAzureResourceManagerRequest(resource, "/some/path");
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName/some/path", pathSuffixRequest.RequestUri.ToString(), "Wrong RequestUri for pathSuffixRequest");

            var noSlashPathSuffixRequest = new TestAzureResourceManagerRequest(resource, "some/path");
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName/some/path", noSlashPathSuffixRequest.RequestUri.ToString(), "Wrong RequestUri for noSlashPathSuffixRequest");

            var querySuffixRequest = new TestAzureResourceManagerRequest(resource, "?param=value");
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName?param=value", querySuffixRequest.RequestUri.ToString(), "Wrong RequestUri for querySuffixRequest");
        }

        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithResourceIdAndQueryThenRequestIsCreated()
        {
            var resource = new ResourceIdentifier(ResourceType.AzureStorage, "subscription", "resourceGroup", "resourceName");
            var aiResource = new ResourceIdentifier(ResourceType.ApplicationInsights, "subscription", "resourceGroup", "resourceName");

            var queryRequest = new TestAzureResourceManagerRequest(resource, "query", TimeSpan.FromHours(5));
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName/providers/microsoft.insights/logs?query=query&timespan=PT5H&api-version=2018-03-01-preview", queryRequest.RequestUri.ToString(), "Wrong RequestUri for queryRequest");

            var queryNoTimeSpanRequest = new TestAzureResourceManagerRequest(resource, "query", null);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Storage/storageAccounts/resourceName/providers/microsoft.insights/logs?query=query&api-version=2018-03-01-preview", queryNoTimeSpanRequest.RequestUri.ToString(), "Wrong RequestUri for queryNoTimeSpanRequest");

            var aiQueryRequest = new TestAzureResourceManagerRequest(aiResource, "query", TimeSpan.FromHours(5));
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Insights/components/resourceName/api/query?query=query&timespan=PT5H&api-version=2018-04-20", aiQueryRequest.RequestUri.ToString(), "Wrong RequestUri for aiQueryRequest");

            var aiQueryNoTimeSpanRequest = new TestAzureResourceManagerRequest(aiResource, "query", null);
            Assert.AreEqual("/subscriptions/subscription/resourceGroups/resourceGroup/providers/Microsoft.Insights/components/resourceName/api/query?query=query&api-version=2018-04-20", aiQueryNoTimeSpanRequest.RequestUri.ToString(), "Wrong RequestUri for aiQueryNoTimeSpanRequest");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithResourceIdAndNullQueryThenExceptionIsThrown()
        {
            var resource = new ResourceIdentifier(ResourceType.AzureStorage, "subscription", "resourceGroup", "resourceName");

            _ = new TestAzureResourceManagerRequest(resource, null, null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenCreatingAzureResourceManagerRequestWithResourceIdAndEmptyQueryThenExceptionIsThrown()
        {
            var resource = new ResourceIdentifier(ResourceType.AzureStorage, "subscription", "resourceGroup", "resourceName");

            _ = new TestAzureResourceManagerRequest(resource, string.Empty, null);
        }

        private class TestAzureResourceManagerRequest : AzureResourceManagerRequest
        {
            public TestAzureResourceManagerRequest(Uri requestUri)
                : base(requestUri)
            {
            }

            public TestAzureResourceManagerRequest(ResourceIdentifier resource, string requestSuffix)
                : base(resource, requestSuffix)
            {
            }

            public TestAzureResourceManagerRequest(ResourceIdentifier resource, string query, TimeSpan? queryTimeSpan)
                : base(resource, query, queryTimeSpan)
            {
            }
        }
    }
}
