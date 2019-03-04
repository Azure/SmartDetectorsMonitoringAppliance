//-----------------------------------------------------------------------
// <copyright file="BaselineServiceClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.BaselineServiceClient
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// An implementation of <see cref="IBaselineServiceClientFactory"/>
    /// </summary>
    public class BaselineServiceClientFactory : IBaselineServiceClientFactory
    {
        private const string CertificateUrl = @"https://cadservicepreviewvault.vault.azure.net/secrets/SmartDiagnosticsNRTClientCert2/ef4d7fcf3d3b44b3841a99ad04e2a413";
        private const string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private const string RedirectUri = "urn:ietf:wg:oauth:2.0:oob";

        private static HttpClient httpClient = null;
        private static SemaphoreSlim httpClientSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Create a new instance of <see cref="IBaselineServiceClient"/>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task{T}"/>, running the current operation, returning the baseline service client</returns>
        public async Task<IBaselineServiceClient> CreateAsync(CancellationToken cancellationToken)
        {
            if (httpClient == null)
            {
                await httpClientSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (httpClient == null)
                    {
                        // Connect to key vault
                        IKeyVaultClient keyVaultClient = new KeyVaultClient(GetToken);

                        // Get the certificate from key vault
                        SecretBundle secretBundle = await keyVaultClient.GetSecretAsync(CertificateUrl, cancellationToken).ConfigureAwait(false);
                        X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(secretBundle.Value));

                        // Create http client
                        HttpClientHandler handler = new HttpClientHandler();
                        handler.ClientCertificates.Add(certificate);
                        httpClient = new HttpClient(handler);
                    }
                }
                finally
                {
                    httpClientSemaphore.Release();
                }
            }

            return new BaselineServiceClient(httpClient);
        }

        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authenticationContext = new AuthenticationContext(authority);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(
                resource,
                ClientId,
                new Uri(RedirectUri),
                new PlatformParameters(PromptBehavior.Auto))
                .ConfigureAwait(false);

            return authenticationResult.AccessToken;
        }
    }
}