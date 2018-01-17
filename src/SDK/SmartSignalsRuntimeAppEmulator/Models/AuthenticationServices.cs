//-----------------------------------------------------------------------
// <copyright file="AuthenticationServices.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System;
    using System.Linq;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// A class providing methods used to authenticate the user with their organization's AAD.
    /// </summary>
    public class AuthenticationServices 
    {
        // The authorization authority used for multi-tenant applications authorizations.
        private const string CommonAuthority = "https://login.microsoftonline.com/common";

        // The resource ID used for authentication requests.
        private const string ResourceId = "https://management.azure.com/";

        // The client ID for the emulator's application - this is registered with Azure, so changing it will break all
        // authentications.
        private const string ClientId = "7696b566-f71e-450a-8681-3b43cec4bef4";

        // The redirect URI registered in Azure for the emulator's application - changing it will break all
        // authentications.
        private static readonly Uri RedirectUri = new Uri("https://azuresmartsignals.microsoft.com");

        // The authentication context used to authenticate with AAD
        private readonly AuthenticationContext authenticationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        public AuthenticationServices()
        {
            // Initialize the AuthenticationContext with the common (tenant-less) endpoint
            this.authenticationContext = new AuthenticationContext(CommonAuthority);

            // If we already have tokens in the cache
            if (this.authenticationContext.TokenCache.ReadItems().Any())
            {
                // Re-bind the AuthenticationContext to the authority that sourced the token in the cache.
                // This is needed for the cache to work when asking a token from that authority
                // (the common endpoint never triggers cache hits)
                string cachedAuthority = this.authenticationContext.TokenCache.ReadItems().First().Authority;
                this.authenticationContext = new AuthenticationContext(cachedAuthority);
            }
        }

        /// <summary>
        /// Gets or sets the authentication result
        /// </summary>
        public AuthenticationResult AuthenticationResult { get; set; }

        #region Implementation of IAuthenticationServices

        /// <summary>
        /// Authenticates the user with their organization's AAD.
        /// </summary>
        public void AuthenticateUser()
        {
            this.AuthenticationResult = this.authenticationContext.AcquireToken(ResourceId, ClientId, RedirectUri, PromptBehavior.Auto);
        }

        #endregion
    }
}
