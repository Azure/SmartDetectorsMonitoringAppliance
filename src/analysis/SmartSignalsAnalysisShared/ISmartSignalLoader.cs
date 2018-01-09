//-----------------------------------------------------------------------
// <copyright file="ISmartSignalLoader.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;

    /// <summary>
    /// An interface used for loading a Smart Signal from its package
    /// </summary>
    public interface ISmartSignalLoader
    {
        /// <summary>
        /// Load a signal from its package
        /// </summary>
        /// <param name="signalPackage">The signal package</param>
        /// <returns>The signal instance</returns>
        ISmartSignal LoadSignal(SmartSignalPackage signalPackage);
    }
}