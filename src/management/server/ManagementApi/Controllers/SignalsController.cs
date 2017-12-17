namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Models;
    using NCrontab;
    using Shared;
    using Shared.Exceptions;
    using Shared.SignalConfiguration;

    /// <summary>
    /// This class is the entry point for the /signals endpoint.
    /// </summary>
    public class SignalsController : ApiController
    {
        private readonly ISmartSignalConfigurationStore smartSignalConfigurationStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsController"/> class.
        /// </summary>
        /// <param name="smartSignalConfigurationStore">The smart signal configuration store.</param>
        public SignalsController(ISmartSignalConfigurationStore smartSignalConfigurationStore)
        {
            this.smartSignalConfigurationStore = smartSignalConfigurationStore;
        }

        /// <summary>
        /// Gets all the smart signals.
        /// </summary>
        /// <returns>The smart signals.</returns>
        [HttpGet]
        [Route("api/v1/signals")]
        public async Task<IEnumerable<SmartSignalConfiguration>> GetAllSmartSignals()
        {
            try
            {
                var smartSignals = await this.smartSignalConfigurationStore.GetAllSmartSignalConfigurationsAsync();

                return smartSignals;
            }
            catch (SmartSignalConfigurationStoreException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get smart signals"));
            }
        }

        /// <summary>
        /// Add the given signal to the smart signal configuration store.
        /// </summary>
        /// <param name="model">The given model with the signal metadata.</param>
        /// <returns>200 if request was successful, 500 if not.</returns>
        [HttpPut]
        [Route("api/v1/signals")]
        public async Task AddSignal([FromBody]AddSignalModel model)
        {
            // Verify given input model is valid
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            CrontabSchedule crontabSchedule = CrontabSchedule.TryParse(model.Schedule);
            if (crontabSchedule == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Schedule value is in wrong format (should be CRON format)"));
            }

            try
            {
                await this.smartSignalConfigurationStore.AddOrReplaceSmartSignalConfigurationAsync(new SmartSignalConfiguration()
                {
                    SignalId = Guid.NewGuid().ToString(),
                    ResourceType = model.ResourceType,
                    Schedule = crontabSchedule
                });
            }
            catch (SmartSignalConfigurationStoreException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to add the given smart signal"));
            }
        }
    }
}
