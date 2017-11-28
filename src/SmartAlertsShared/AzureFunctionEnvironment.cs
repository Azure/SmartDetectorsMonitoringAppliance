// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//         Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
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
        /// Name of the website
        /// </summary>
        public static string WebAppSiteName => System.Environment.GetEnvironmentVariable(WebsiteNameKey);

        /// <summary>
        /// Name of the hostname used to run this function
        /// </summary>
        public static string HostName => System.Environment.GetEnvironmentVariable(HostNameKey);

        /// <summary>
        /// The website instance Id that runs this job. This is a generated GUID as opposed to the Hostname that can be any string.
        /// </summary>
        public static string WebsiteInstanceId => System.Environment.GetEnvironmentVariable(WebsiteInstanceIdKey);

        /// <summary>
        /// Whether we are running locally
        /// </summary>
        public static bool IsLocalEnvironment => string.IsNullOrEmpty(WebsiteInstanceId);
    }
}