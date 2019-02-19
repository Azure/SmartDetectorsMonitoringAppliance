//-----------------------------------------------------------------------
// <copyright file="AuthenticationServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// A class providing methods used to authenticate the user with their organization's AAD.
    /// </summary>
    public class AuthenticationServices : IAuthenticationServices
    {
        // Used to update expired AuthenticationResult in a thread safe way
        private static readonly SemaphoreSlim AuthenticationSemaphoreSlim = new SemaphoreSlim(1, 1);

        // The resource ID used for authentication requests.
        private readonly string resourceId;

        // The client ID for the emulator's application.
        private readonly string clientId;

        // The redirect URI registered in Azure for the emulator's application.
        private readonly Uri redirectUri;

        // The authentication context used to authenticate with AAD
        private readonly AuthenticationContext authenticationContext;

        // The AAD authetication result
        private AuthenticationResult authenticationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        public AuthenticationServices()
        {
            this.AuthenticatedUserName = string.Empty;

            string commonAuthority = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["CommonAuthority"]);
            this.resourceId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["ResourceId"]);
            this.clientId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["ClientId"]);
            this.redirectUri = new Uri(Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["RedirectUri"]));

            // Initialize the AuthenticationContext with the common (tenant-less) endpoint
            this.authenticationContext = new AuthenticationContext(commonAuthority);

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

        #region IAuthenticationServices implementation

        /// <summary>
        /// Gets the authenticated user name.
        /// </summary>
        public string AuthenticatedUserName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether access token is about to expire
        /// </summary>
        private bool IsAccessTokenAboutToExpire => this.authenticationResult.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5);

        /// <summary>
        /// Authenticates the user with their organization's AAD.
        /// This method is not thread safe.
        /// </summary>
        /// <returns>A <see cref="Task"/> running the async operation.</returns>
        public async Task AuthenticateUserAsync()
        {
            this.authenticationResult = await this.authenticationContext.AcquireTokenAsync(
                this.resourceId,
                this.clientId,
                this.redirectUri,
                new PlatformParameters(PromptBehavior.Auto, null),
                UserIdentifier.AnyUser,
                "prompt=consent");

            this.AuthenticatedUserName = this.authenticationResult.UserInfo.GivenName;
        }

        /// <summary>
        /// Get access token for a resource.
        /// This method is thread safe.
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <returns>A <see cref="Task"/> that returns the resource's access token</returns>
        public async Task<string> GetResourceTokenAsync(string resource)
        {
            if (this.IsAccessTokenAboutToExpire)
            {
                await AuthenticationSemaphoreSlim.WaitAsync(TimeSpan.FromMinutes(1));
                try
                {
                    // Check again
                    if (this.IsAccessTokenAboutToExpire)
                    {
                        await this.AuthenticateUserAsync();
                    }
                }
                finally
                {
                    AuthenticationSemaphoreSlim.Release();
                }
            }

            var authResult = await this.authenticationContext.AcquireTokenAsync(
                resource,
                this.clientId,
                new UserAssertion(this.authenticationResult.AccessToken, this.authenticationResult.AccessTokenType));

            return authResult.AccessToken;
        }

        #endregion
    }
}
