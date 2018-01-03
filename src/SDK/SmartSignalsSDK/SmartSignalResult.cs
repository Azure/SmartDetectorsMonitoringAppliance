//-----------------------------------------------------------------------
// <copyright file="SmartSignalResult.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System.Collections.Generic;

    /// <summary>
    /// A class representing the result of a specific Smart Signal execution.
    /// </summary>
    public sealed class SmartSignalResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalResult" /> class 
        /// </summary>
        public SmartSignalResult()
        {
            this.ResultItems = new List<SmartSignalResultItem>();
        }

        /// <summary>
        /// Gets the list of Smart Signal result items that were produced in the signal's execution
        /// </summary>
        public List<SmartSignalResultItem> ResultItems { get; }
    }
}
