//-----------------------------------------------------------------------
// <copyright file="ActiveDirectoryCredentialsFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using Microsoft.Azure.Monitoring.SmartSignals.Common;
    using Microsoft.Rest;

    /// <summary>
    /// An implementation of the <see cref="ICredentialsFactory"/>, creating Azure Active Directory credentials.
    /// </summary>
    public class ActiveDirectoryCredentialsFactory : ICredentialsFactory
    {
        private readonly string accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryCredentialsFactory"/> class.
        /// </summary>
        /// <param name="accessToken">The authentication result access token</param>
        public ActiveDirectoryCredentialsFactory(string accessToken)
        {
            this.accessToken = accessToken;
        }

        /// <summary>
        /// Create an instance of the <see cref="ServiceClientCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        public ServiceClientCredentials Create(string resource)
        {
            return new ActiveDirectoryCredentials(this.accessToken);
        }
    }
}
