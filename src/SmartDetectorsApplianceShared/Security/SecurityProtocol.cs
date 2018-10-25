//-----------------------------------------------------------------------
// <copyright file="SecurityProtocol.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Security
{
    using System;
    using System.Net;

    /// <summary>
    /// A class contains all the methods related to security protocols
    /// </summary>
    public static class SecurityProtocol
    {
        /// <summary>
        /// Force the use of the most updated SSL/TLS version (currently 1.2) and remove all the older versions.
        /// </summary>
        public static void RemoveUnsecureProtocols()
        {
            foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
            {
                switch (protocol)
                {
                    // Add any un-secure versions here
                    case SecurityProtocolType.Ssl3:
                    case SecurityProtocolType.Tls:
                    case SecurityProtocolType.Tls11:
                    {
                        if ((ServicePointManager.SecurityProtocol & protocol) == protocol)
                        {
                            ServicePointManager.SecurityProtocol ^= protocol;
                        }

                        break;
                    }

                    default:
                    {
                        // Secure version that is not included above will be used here
                        ServicePointManager.SecurityProtocol |= protocol;

                        break;
                    }
                }
            }
        }
    }
}
