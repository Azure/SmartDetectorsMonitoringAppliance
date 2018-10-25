//-----------------------------------------------------------------------
// <copyright file="IAuthenticationServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for Azure authentication services.
    /// </summary>
    public interface IAuthenticationServices
    {
        /// <summary>
        /// Gets the authenticated user name.
        /// </summary>
        string AuthenticatedUserName { get; }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        void AuthenticateUser();

        /// <summary>
        /// Get access token for a resource.
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <returns>A task that returns the resource's access token</returns>
        Task<string> GetResourceTokenAsync(string resource);
    }
}