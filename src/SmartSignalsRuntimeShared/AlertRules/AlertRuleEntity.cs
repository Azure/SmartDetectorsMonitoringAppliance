//-----------------------------------------------------------------------
// <copyright file="AlertRuleEntity.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the alert rule for the smart signal.
    /// The rule ID is the row key.
    /// </summary>
    public class AlertRuleEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the signal ID.
        /// </summary>
        public string SignalId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the resource applicable for the signal.
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the signal's schedule
        /// </summary>
        public string CrontabSchedule { get; set; }
    }
}
