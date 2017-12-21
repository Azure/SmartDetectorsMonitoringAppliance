namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Models;
    using NCrontab;
    using Responses;
    using Shared;
    using Shared.Exceptions;
    using Shared.Models;
    using Shared.SignalConfiguration;

    /// <summary>
    /// This class is the logic for the /signals endpoint.
    /// </summary>
    public class SignalsLogic : ISignalsLogic
    {
        private readonly ISmartSignalsRepository smartSignalsRepository;
        private readonly ISmartSignalConfigurationStore smartSignalConfigurationStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsLogic"/> class.
        /// </summary>
        /// <param name="smartSignalsRepository">The smart signal repository.</param>
        /// <param name="smartSignalConfigurationStore">The smart signal configuration store.</param>
        public SignalsLogic(ISmartSignalsRepository smartSignalsRepository, ISmartSignalConfigurationStore smartSignalConfigurationStore)
        {
            Diagnostics.EnsureArgumentNotNull(() => smartSignalsRepository);
            Diagnostics.EnsureArgumentNotNull(() => smartSignalConfigurationStore);

            this.smartSignalsRepository = smartSignalsRepository;
            this.smartSignalConfigurationStore = smartSignalConfigurationStore;
        }

        /// <summary>
        /// List all the smart signals.
        /// </summary>
        /// <returns>The smart signals.</returns>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to retrieve smart signals.</exception>
        public async Task<ListSmartSignalsResponse> GetAllSmartSignalsAsync()
        {
            try
            {
                IList<SmartSignalMetadata> smartSignals = await this.smartSignalsRepository.ReadAllSignalsMetadataAsync();

                // Convert smart signals to the required response
                var signals = smartSignals.Select(metadata => new Signal
                {
                   Id = metadata.Id,
                   Name = metadata.Name,
                   SupportedCadences = new List<int>(),  // TODO - wait for Aviram to complete this
                   Configurations = new List<SignalConfiguration>()
                }).ToList();

                return new ListSmartSignalsResponse()
                {
                    Signals = signals
                };
            }
            catch (SmartSignalConfigurationStoreException e)
            {
                throw new SmartSignalsManagementApiException("Failed to get smart signals", e, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Add the given signal to the smart signal versions store.
        /// </summary>
        /// <returns>A task represents this operation.</returns>
        /// <param name="addSignalVersion">The model that contains all the require parameters for adding signal version.</param>
        /// <exception cref="SmartSignalsManagementApiException">This exception is thrown when we failed to add smart signals version.</exception>
        public async Task AddSignalVersionAsync(AddSignalVersion addSignalVersion)
        {
            Diagnostics.EnsureArgumentNotNull(() => addSignalVersion);

            // Verify given input model is valid
            if (!IsAddSignalVersionModelValid(addSignalVersion, out var validationError))
            {
                throw new SmartSignalsManagementApiException(validationError, HttpStatusCode.BadRequest);
            }

            try
            {
                await this.smartSignalConfigurationStore.AddOrReplaceSmartSignalConfigurationAsync(new SmartSignalConfiguration()
                {
                    SignalId = addSignalVersion.SignalId,
                    ResourceType = addSignalVersion.ResourceType,
                    Schedule = CrontabSchedule.Parse(addSignalVersion.Schedule)
                });
            }
            catch (SmartSignalConfigurationStoreException e)
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
        private static bool IsAddSignalVersionModelValid(AddSignalVersion model, out string errorInformation)
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
