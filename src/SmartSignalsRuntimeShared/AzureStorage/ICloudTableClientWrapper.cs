//-----------------------------------------------------------------------
// <copyright file="ICloudTableClientWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AzureStorage
{
    /// <summary>
    /// An interface for an Azure Storage table client wrapper
    /// </summary>
    public interface ICloudTableClientWrapper
    {
        /// <summary>
        /// Gets a reference to the specified table.
        /// </summary>
        /// <param name="tableName">A string containing the name of the table.</param>
        /// <returns>A <see cref="ICloudTableWrapper"/> object.</returns>
        ICloudTableWrapper GetTableReference(string tableName);
    }
}
