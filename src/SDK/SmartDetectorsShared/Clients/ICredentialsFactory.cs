﻿//-----------------------------------------------------------------------
// <copyright file="ICredentialsFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Rest;

    /// <summary>
    /// An interface for a client credentials factory
    /// </summary>
    public interface ICredentialsFactory
    {
        /// <summary>
        /// Create an instance of the <see cref="ServiceClientCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        ServiceClientCredentials CreateServiceClientCredentials(string resource);

        /// <summary>
        /// Create an instance of the <see cref="AzureCredentials"/> class.
        /// </summary>
        /// <param name="resource">The resource for which to create the credentials</param>
        /// <returns>The credentials</returns>
        AzureCredentials CreateAzureCredentials(string resource);
    }
}