namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Threading.Tasks;
    using System.Net;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Models;
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
        /// <param name="alertRuleStore">The smart signal configuration store.</param>
        public AlertRuleApi(IAlertRuleStore alertRuleStore)
        {
            Diagnostics.EnsureArgumentNotNull(() => alertRuleStore);

            this.alertRuleStore = alertRuleStore;
        }

        /// <summary>
        /// Add the given alert rule to the alert rules store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addAlertRule">The model that contains all the require parameters for adding signal version.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add smart signals version.</exception>
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
                throw new SmartSignalsManagementApiException("Failed to add the given smart signal configuration", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Validates if the given model for adding signal configuration is valid.
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
