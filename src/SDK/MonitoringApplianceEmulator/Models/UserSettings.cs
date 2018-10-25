//-----------------------------------------------------------------------
// <copyright file="UserSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the user settings. This class is initialized from the user settings file.
    /// In case the expected file doesn't exist, all properties are initialized with <code>null</code> values.
    /// </summary>
    public class UserSettings
    {
        private static readonly object FileLock = new object();

        private static readonly string UserSettingsFilePath = GenerateUserSettingsPath();

        /// <summary>
        /// Gets or sets the selected subscription.
        /// </summary>
        [JsonProperty("selectedSubscription", Required = Required.AllowNull)]
        public string SelectedSubscription { get; set; }

        /// <summary>
        /// Gets or sets the selected resource type.
        /// </summary>
        [JsonProperty("selectedResourceType", Required = Required.AllowNull)]
        public string SelectedResourceType { get; set; }

        /// <summary>
        /// Gets or sets the selected resource.
        /// </summary>
        [JsonProperty("selectedResource", Required = Required.AllowNull)]
        public string SelectedResource { get; set; }

        /// <summary>
        /// Loads the user settings from a file.
        /// </summary>
        /// <returns>A new instance of <see cref="UserSettings"/>.</returns>
        public static UserSettings LoadUserSettings()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(UserSettingsFilePath))
                    {
                        using (StreamReader reader = File.OpenText(UserSettingsFilePath))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            return (UserSettings)serializer.Deserialize(reader, typeof(UserSettings));
                        }
                    }
                }
                catch (Exception)
                {
                    // Swallow exception and don't do anything.
                }
            }

            return new UserSettings();
        }

        /// <summary>
        /// Saves the user settings to a file.
        /// </summary>
        public void Save()
        {
            lock (FileLock)
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(UserSettingsFilePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, this);
                    }
                }
                catch (Exception)
                {
                    // Swallow exception and don't do anything.
                }
            }
        }

        /// <summary>
        /// Generates the full path (including the name) of the file that should contain user settings.
        /// </summary>
        /// <returns>the file path</returns>
        private static string GenerateUserSettingsPath()
        {
            // Get the folder for the roaming current user
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Creates a 'SmartAlertsEmulator' folder
            string emulatorPath = Path.Combine(appDataFolderPath, "SmartAlertsEmulator");

            return Path.Combine(emulatorPath, "UserSettings.json");
        }
    }
}
