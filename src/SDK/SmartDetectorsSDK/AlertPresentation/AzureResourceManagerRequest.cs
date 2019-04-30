//-----------------------------------------------------------------------
// <copyright file="AzureResourceManagerRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.AlertPresentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// Base class for alert properties that get their data from the result of an ARM request.
    /// Inheriting classes should add presentation properties to display the results of the ARM request.
    /// </summary>
    public abstract class AzureResourceManagerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerRequest"/> class.
        /// </summary>
        /// <param name="requestUri">The request's URI. This must be a relative URI that will be executed against the ARM endpoint.</param>
        protected AzureResourceManagerRequest(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (requestUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The ARM request URI must be a relative URI", nameof(requestUri));
            }

            this.RequestUri = requestUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerRequest"/> class, based on the given resource and suffix.
        /// </summary>
        /// <param name="resource">The resource for which the ARM request should be made</param>
        /// <param name="requestSuffix">An optional suffix for the request, which will appended to the resource's URI</param>
        protected AzureResourceManagerRequest(ResourceIdentifier resource, string requestSuffix)
        {
            // Make sure that we'll be able to append the request suffix
            if (string.IsNullOrEmpty(requestSuffix))
            {
                this.RequestUri = new Uri(resource.ToResourceId(), UriKind.Relative);
            }
            else if (requestSuffix[0] != '/' && requestSuffix[0] != '?')
            {
                this.RequestUri = new Uri($"{resource.ToResourceId()}/{requestSuffix}", UriKind.Relative);
            }
            else
            {
                this.RequestUri = new Uri($"{resource.ToResourceId()}{requestSuffix}", UriKind.Relative);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureResourceManagerRequest"/> class, based on an Azure Monitor log
        /// query.
        /// </summary>
        /// <param name="resource">The resource for which to make the query</param>
        /// <param name="query">The query to run</param>
        /// <param name="queryTimeSpan">Optional time span to use for limiting the query data.</param>
        protected AzureResourceManagerRequest(ResourceIdentifier resource, string query, TimeSpan? queryTimeSpan)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            // Compose the request URL (different handling for Application Insights vs. Log Analytics)
            string apiVersion;
            string queryUrlPath;
            if (resource.ResourceType == ResourceType.ApplicationInsights)
            {
                // Applications Insights query API format is: {resource}/api/query?query={query}&timespan={queryTimeSpan}&api-version=2018-04-20
                apiVersion = "2018-04-20";
                queryUrlPath = $"{resource.ToResourceId()}/api/query";
            }
            else
            {
                // Log Analytics query API format is: {resource}/providers/microsoft.insights/logs?query={query}&timespan={queryTimeSpan}&api-version=2018-03-01-preview
                apiVersion = "2018-03-01-preview";
                queryUrlPath = $"{resource.ToResourceId()}/providers/microsoft.insights/logs";
            }

            // Create the list of query parameters, and serialize to a string
            var urlQueryParameters = new List<string>
            {
                $"query={Uri.EscapeDataString(query)}",
                queryTimeSpan.HasValue ? $"timespan={XmlConvert.ToString(queryTimeSpan.Value)}" : null,
                $"api-version={apiVersion}"
            };

            string urlQueryParametersString = string.Join("&", urlQueryParameters.Where(param => !string.IsNullOrWhiteSpace(param)));

            // And compose the URI
            this.RequestUri = new Uri($"{queryUrlPath}?{urlQueryParametersString}", UriKind.Relative);
        }

        /// <summary>
        /// Gets the ARM request URI. This is a relative URI that will be executed against the ARM
        /// endpoint of the current Azure environment. For example, to query the list of activity logs
        /// in a specific subscription, the URI should be
        /// <c>/subscriptions/089bd33f-d4ec-47fe-8ba5-0753aa5c5b33/providers/microsoft.insights/eventtypes/management/values?api-version=2015-04-01</c>
        /// </summary>
        public Uri RequestUri { get; }
    }
}
