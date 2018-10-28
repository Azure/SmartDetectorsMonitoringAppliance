//-----------------------------------------------------------------------
// <copyright file="PageableLogArchiveFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace
{
    /// <summary>
    /// An implementation of the <see cref="IPageableLogArchiveFactory"/> interface
    /// </summary>
    public class PageableLogArchiveFactory : IPageableLogArchiveFactory
    {
        #region Implementation of IPageableLogArchiveFactory

        /// <summary>
        /// Creates a new instance of the <see cref="IPageableLogArchive"/> interface.
        /// </summary>
        /// <returns>The newly created instance</returns>
        public IPageableLogArchive Create()
        {
            return new PageableLogArchive();
        }

        #endregion
    }
}
