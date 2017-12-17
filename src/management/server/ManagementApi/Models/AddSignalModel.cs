namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// This class represents the model of the PUT signals request body.
    /// </summary>
    public class AddSignalModel
    {
        /// <summary>
        /// Gets or sets the resource type supported by the signal.
        /// </summary>
        [Required(ErrorMessage = "Resource type must be non-empty and have a valid value")]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the scheduling configuration (in CRON format).
        /// </summary>
        [Required(ErrorMessage = "Schedule value must be non-empty and have a valid value (in CRON format)")]
        public string Schedule { get; set; }
    }
}