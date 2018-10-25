//-----------------------------------------------------------------------
// <copyright file="AzureFunctionEnvironment.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Trace
{
    /// <summary>
    /// A static class holding Azure function environment variables
    /// </summary>
    public static class AzureFunctionEnvironment
    {
        private const string WebsiteNameKey = "WEBSITE_SITE_NAME";
        private const string HostNameKey = "WEBSITE_HOSTNAME";
        private const string WebsiteInstanceIdKey = "WEBSITE_INSTANCE_ID";

        /// <summary>
        /// Gets the name of the website
        /// </summary>
        public static string WebAppSiteName => System.Environment.GetEnvironmentVariable(WebsiteNameKey);

        /// <summary>
        /// Gets the name of the host used to run this function
        /// </summary>
        public static string HostName => System.Environment.GetEnvironmentVariable(HostNameKey);

        /// <summary>
        /// Gets the website instance Id that runs this job. This is a generated GUID as opposed to the Hostname that can be any string.
        /// </summary>
        public static string WebsiteInstanceId => System.Environment.GetEnvironmentVariable(WebsiteInstanceIdKey);

        /// <summary>
        /// Gets a value indicating whether we are running locally
        /// </summary>
        public static bool IsLocalEnvironment => string.IsNullOrEmpty(WebsiteInstanceId);
    }
}