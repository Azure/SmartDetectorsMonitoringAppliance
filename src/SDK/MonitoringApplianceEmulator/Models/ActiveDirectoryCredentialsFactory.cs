//-----------------------------------------------------------------------
// <copyright file="ActiveDirectoryCredentialsFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Rest;

    /// <summary>
    /// An implementation of the <see cref="ICredentialsFactory"/>, creating Azure Active Directory credentials.
    /// </summary>
    public class ActiveDirectoryCredentialsFactory : ICredentialsFactory
    {
        private readonly IAuthenticationServices authenticationServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryCredentialsFactory"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services</param>
        public ActiveDirectoryCredentialsFactory(IAuthenticationServices authenticationServices)
        {
            this.authenticationServices = authenticationServices;
        }

        /// <summary>
        /// Create an instance of the <see cref="ServiceClientCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public ServiceClientCredentials CreateServiceClientCredentials(string resource)
        {
            return new ActiveDirectoryCredentials(this.authenticationServices, resource);
        }

        /// <summary>
        /// Create an instance of the <see cref="AzureCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public AzureCredentials CreateAzureCredentials(string resource)
        {
            var activeDirectoryCredentials = new ActiveDirectoryCredentials(this.authenticationServices, resource);

            return new AzureCredentials(
                armCredentials: activeDirectoryCredentials,
                graphCredentials: activeDirectoryCredentials,
                tenantId: "microsoft.com",
                environment: AzureEnvironment.AzureGlobalCloud);
        }
    }
}
