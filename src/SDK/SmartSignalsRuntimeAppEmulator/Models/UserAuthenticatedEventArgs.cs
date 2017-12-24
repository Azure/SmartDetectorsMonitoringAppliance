//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public delegate void UserAuthenticatedEventHandler(object sender, UserAuthenticatedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="UserAuthenticatedEventHandler" /> event handler.
    /// </summary>
    public class UserAuthenticatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the user info of the authenticated user.
        /// </summary>
        public UserInfo UserInfo { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticatedEventArgs"/> class.
        /// </summary>
        /// <param name="userInfo">The info of the authenticated user, or null if non exists.</param>
        public UserAuthenticatedEventArgs(UserInfo userInfo)
        {
            this.UserInfo = userInfo;
        }
    }
}
