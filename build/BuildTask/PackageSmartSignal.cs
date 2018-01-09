//-----------------------------------------------------------------------
// <copyright file="PackageSmartSignal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Build
{
    using Microsoft.Azure.Monitoring.SmartSignals.Package;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Represents the build task of a Smart Signal
    /// </summary>
    public class PackageSmartSignal : Task
    {
        /// <summary>
        /// Executes PackageSmartSignal task. 
        /// </summary>
        /// <returns>True if the task successfully executed; otherwise, False.</returns>
        public override bool Execute()
        {
            bool result = false;
            if (!result)
            {
                Log.LogError("Failed");
            }

            return result;
        }
    }
}
