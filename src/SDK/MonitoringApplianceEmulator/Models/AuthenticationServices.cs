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
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// A class providing methods used to authenticate the user with their organization's AAD.
    /// </summary>
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly string directoryId;

        // The resource ID used for authentication requests.
        private readonly string resourceId;

        // The client ID for the emulator's application.
        private readonly string clientId;

        // The client secret
        private readonly string clientSecret;

        // The redirect URI registered in Azure for the emulator's application.
        private readonly Uri redirectUri;

        // The authentication context used to authenticate with AAD
        private readonly AuthenticationContext authenticationContext;

        // The lock, used to update expired AuthenticationResult in a thread safe way
        private readonly object authenticationResultLock = new object();

        // The AAD authetication result
        private AuthenticationResult authenticationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        /// <param name="directoryId">The directory id</param>
        /// <param name="clientId">The client id</param>
        /// <param name="clientSecret">The client secret</param>
        public AuthenticationServices(string directoryId, string clientId, string clientSecret)
        {
            this.directoryId = directoryId;
            this.AuthenticatedUserName = string.Empty;

            string commonAuthority = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["CommonAuthority"]);
            this.resourceId = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["ResourceId"]);
            this.clientId = clientId;
            this.redirectUri = new Uri(Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["RedirectUri"]));
            this.clientSecret = clientSecret;

            // Initialize the AuthenticationContext with the common (tenant-less) endpoint
            if (string.IsNullOrEmpty(directoryId))
            {
                this.authenticationContext = new AuthenticationContext(commonAuthority);
            }
            else
            {
                this.authenticationContext = new AuthenticationContext($@"https://login.windows.net/{this.directoryId}/");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        public AuthenticationServices()
            : this(
                null,
                Diagnostics.EnsureStringNotNullOrWhiteSpace(() => ConfigurationManager.AppSettings["ClientId"]),
                null)
        {
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
        public void AuthenticateUser()
        {
            if (string.IsNullOrEmpty(this.clientSecret))
            {
                this.authenticationResult = this.authenticationContext.AcquireToken(
                    this.resourceId,
                    this.clientId,
                    this.redirectUri,
                    PromptBehavior.Auto);
                ////UserIdentifier.AnyUser,
                ////"prompt=consent");
            }
            else
            {
                this.authenticationResult = this.authenticationContext.AcquireToken(
                    this.resourceId,
                    new ClientCredential(this.clientId, this.clientSecret));
            }

            this.AuthenticatedUserName = this.authenticationResult.UserInfo?.GivenName;
        }

        /// <summary>
        /// Get access token for a resource.
        /// This method is thread safe.
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <returns>A task that returns the resource's access token</returns>
        public async Task<string> GetResourceTokenAsync(string resource)
        {
            if (this.IsAccessTokenAboutToExpire)
            {
                lock (this.authenticationResultLock)
                {
                    // Check again
                    if (this.IsAccessTokenAboutToExpire)
                    {
                        if (string.IsNullOrEmpty(this.clientSecret))
                        {
                            this.authenticationResult =
                                this.authenticationContext.AcquireTokenByRefreshToken(
                                    this.authenticationResult.RefreshToken, this.clientId);
                        }
                        else
                        {
                            this.authenticationResult =
                                this.authenticationContext.AcquireTokenByRefreshToken(
                                    this.authenticationResult.RefreshToken, new ClientCredential(this.clientId, this.clientSecret));
                        }
                    }
                }
            }

            AuthenticationResult authResult;
            if (string.IsNullOrEmpty(this.clientSecret))
            {
                authResult = await this.authenticationContext.AcquireTokenAsync(
                    resource,
                    this.clientId,
                    new UserAssertion(
                        this.authenticationResult.AccessToken,
                        this.authenticationResult.AccessTokenType));
            }
            else
            {
                authResult = await this.authenticationContext.AcquireTokenAsync(
                    resource,
                    new ClientCredential(this.clientId, this.clientSecret));
            }

            return authResult.AccessToken;
        }

        #endregion
    }
}
