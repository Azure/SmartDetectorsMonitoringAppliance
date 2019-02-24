//-----------------------------------------------------------------------
// <copyright file="AuthenticationServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// A class providing methods used to authenticate the user with their organization's AAD.
    /// </summary>
    public class AuthenticationServices : IAuthenticationServices
    {
        private static readonly SemaphoreSlim AuthenticationSemaphoreSlim = new SemaphoreSlim(1, 1);

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

        // The AAD authentication result
        private AuthenticationResult authenticationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        /// <param name="directoryId">The directory id</param>
        /// <param name="clientId">The client id</param>
        /// <param name="clientSecret">The client secret</param>
        public AuthenticationServices(string directoryId, string clientId, string clientSecret)
        {
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
                this.authenticationContext = new AuthenticationContext($@"https://login.windows.net/{directoryId}/");
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
        /// <returns>A <see cref="Task"/> running the async operation.</returns>
        public async Task AuthenticateUserAsync()
        {
            if (string.IsNullOrEmpty(this.clientSecret))
            {
                this.authenticationResult = await this.authenticationContext.AcquireTokenAsync(
                    this.resourceId,
                    this.clientId,
                    this.redirectUri,
                    new PlatformParameters(PromptBehavior.Auto, null));
                    ////UserIdentifier.AnyUser,
                    ////"prompt=consent");
            }
            else
            {
                this.authenticationResult = await this.authenticationContext.AcquireTokenAsync(
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
        /// <returns>A <see cref="Task"/> that returns the resource's access token</returns>
        public async Task<string> GetResourceTokenAsync(string resource)
        {
            if (this.IsAccessTokenAboutToExpire && string.IsNullOrEmpty(this.clientSecret))
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
