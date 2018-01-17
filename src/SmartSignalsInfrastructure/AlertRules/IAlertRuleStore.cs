//-----------------------------------------------------------------------
// <copyright file="IAlertRuleStore.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AlertRules
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface providing methods for retrieving and saving an alert rule of a smart signal.
    /// </summary>
    public interface IAlertRuleStore
    {
        /// <summary>
        /// Gets all the alert rules from the store.
        /// </summary>
        /// <returns>A <see cref="IList{AlertRule}"/> containing all the alert rules in the store.</returns>
        Task<IList<AlertRule>> GetAllAlertRulesAsync();

        /// <summary>
        /// Adds or updates an alert rule in the store.
        /// </summary>
        /// <param name="alertRule">The alert rule to add to the store.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> object that represents the asynchronous operation.</returns>
        Task AddOrReplaceAlertRuleAsync(AlertRule alertRule);
    }
}
