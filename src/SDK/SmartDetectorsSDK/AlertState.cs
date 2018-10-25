//-----------------------------------------------------------------------
// <copyright file="AlertState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    /// <summary>
    /// Enumeration of all possible Alert states.
    /// </summary>
    public enum AlertState
    {
        /// <summary>
        /// The Alert is active
        /// </summary>
        Active = 0,

        /// <summary>
        /// The alert is resolved
        /// </summary>
        Resolved = 1,
    }
}
