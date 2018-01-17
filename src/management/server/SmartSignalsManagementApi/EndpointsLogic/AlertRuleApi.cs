//-----------------------------------------------------------------------
// <copyright file="AlertRuleApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.Exceptions;
    using NCrontab;

    /// <summary>
    /// This class is the logic for the /alertRule endpoint.
    /// </summary>
    public class AlertRuleApi : IAlertRuleApi
    {
        private readonly IAlertRuleStore alertRuleStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertRuleApi"/> class.
        /// </summary>
        /// <param name="alertRuleStore">The alert rules store.</param>
        public AlertRuleApi(IAlertRuleStore alertRuleStore)
        {
            Diagnostics.EnsureArgumentNotNull(() => alertRuleStore);

            this.alertRuleStore = alertRuleStore;
        }

        /// <summary>
        /// Add the given alert rule to the alert rules store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addAlertRule">The model that contains all the require parameters for adding alert rule.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add the alert rule.</exception>
        public async Task AddAlertRuleAsync(AddAlertRule addAlertRule)
        {
            Diagnostics.EnsureArgumentNotNull(() => addAlertRule);

            // Verify given input model is valid
            if (!IsAddAlertRuleModelValid(addAlertRule, out var validationError))
            {
                throw new SmartSignalsManagementApiException(validationError, HttpStatusCode.BadRequest);
            }

            try
            {
                await this.alertRuleStore.AddOrReplaceAlertRuleAsync(new AlertRule()
                {
                    Id = Guid.NewGuid().ToString(),
                    SignalId = addAlertRule.SignalId,
                    ResourceType = addAlertRule.ResourceType,
                    Schedule = CrontabSchedule.Parse(addAlertRule.Schedule)
                });
            }
            catch (AlertRuleStoreException e)
            {
                throw new SmartSignalsManagementApiException("Failed to add the given alert rule", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Validates if the given model for adding alert rule is valid.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="errorInformation">The error information which will be filled in case validation will fail.</param>
        /// <returns>True in case model is valid, else false.</returns>
        private static bool IsAddAlertRuleModelValid(AddAlertRule model, out string errorInformation)
        {
            if (string.IsNullOrWhiteSpace(model.SignalId))
            {
                errorInformation = "Signal ID can't be empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(model.Schedule))
            {
                errorInformation = "Schedule parameter must not be empty";
                return false;
            }

            CrontabSchedule crontabSchedule = CrontabSchedule.TryParse(model.Schedule);
            if (crontabSchedule == null)
            {
                errorInformation = "Schedule parameter is not in CRON format";
                return false;
            }

            errorInformation = string.Empty;
            return true;
        }
    }
}
