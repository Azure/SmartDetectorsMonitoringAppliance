//-----------------------------------------------------------------------
// <copyright file="EmulationRunSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the emulation run settings.
    /// </summary>
    public class EmulationRunSettings
    {
        private static readonly string EmulationAlertsFilePath = GenerateEmulationAlertsPath();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationRunSettings"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="analysisCadence">The analysis cadence.</param>
        /// <param name="emulationAlerts">The emulation alerts.</param>
        /// <param name="userSettings">The user settings.</param>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="iterativeRunModeEnabled">Indicates whether the iterative run mode is enabled.</param>
        public EmulationRunSettings(DateTime startTime, DateTime endTime, SmartDetectorCadence analysisCadence, List<EmulationAlert> emulationAlerts, UserSettings userSettings, string subscriptionId, bool iterativeRunModeEnabled)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.AnalysisCadence = analysisCadence;
            this.EmulationAlerts = emulationAlerts;
            this.UserSettings = userSettings;
            this.SubscriptionId = subscriptionId;
            this.IterativeRunModeEnabled = iterativeRunModeEnabled;
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        [JsonProperty("startTime", Required = Required.AllowNull)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        [JsonProperty("endTime", Required = Required.AllowNull)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the analysis cadence.
        /// </summary>
        [JsonProperty("analysisCadence", Required = Required.AllowNull)]
        public SmartDetectorCadence AnalysisCadence { get; set; }

        /// <summary>
        /// Gets the Emulation Alerts collection.
        /// </summary>
        [JsonProperty("emulationAlerts", Required = Required.AllowNull)]
        public List<EmulationAlert> EmulationAlerts { get; }

        /// <summary>
        /// Gets or sets the user settings.
        /// </summary>
        [JsonProperty("userSettings", Required = Required.Always)]
        public UserSettings UserSettings { get; set; }

        /// <summary>
        /// Gets the subscription ID.
        /// </summary>
        [JsonProperty("subscriptionId", Required = Required.Always)]
        public string SubscriptionId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the iterative run mode is enabled.
        /// </summary>
        [JsonProperty("iterativeRunModeEnabled", Required = Required.Always)]
        public bool IterativeRunModeEnabled { get; set; }

        /// <summary>
        /// Loads the emulation run settings from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A new instance of <see cref="EmulationRunSettings"/>.</returns>
        public static EmulationRunSettings LoadEmulationRunSettings(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = File.OpenText(filePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return (EmulationRunSettings)serializer.Deserialize(reader, typeof(EmulationRunSettings));
                    }
                }
            }
            catch (Exception exception)
            {
                var message = $"Failed to load the file: {exception.Message}";
                MessageBox.Show(message);
            }

            return null;
        }

        /// <summary>
        /// Saves the emulation run settings to a file.
        /// </summary>
        public void Save()
        {
            try
            {
                string emulatorRunSettingsPath = Path.Combine(EmulationAlertsFilePath, $"{this.SubscriptionId}");

                // Creates a folder for saving the emulation run settings
                Directory.CreateDirectory(emulatorRunSettingsPath);

                // Creates the path of the file with the emulation run settings
                string filePath = Path.Combine(emulatorRunSettingsPath, $"{this.StartTime:yyyy-MM-dd HH-mm-ss}_{this.EndTime:yyyy-MM-dd HH-mm-ss}_{this.AnalysisCadence.TimeSpan:hh'-'mm'-'ss}_{this.EmulationAlerts.Count}.json");

                using (StreamWriter writer = File.CreateText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception exception)
            {
                var message = $"Failed to save the file: {exception.Message}";
                MessageBox.Show(message);
            }
        }

        /// <summary>
        /// Generates the path of the folder that should contain emulator run settings folder.
        /// </summary>
        /// <returns>The file path</returns>
        private static string GenerateEmulationAlertsPath()
        {
            // Get the folder for the roaming current user
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(appDataFolderPath, "SmartAlertsEmulator");
        }
    }
}
