//-----------------------------------------------------------------------
// <copyright file="ConfigurationReader.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A static class for reading the system's configuration
    /// </summary>
    public static class ConfigurationReader
    {
        /// <summary>
        /// This method reads configuration settings from app.config.
        /// This should be used for Non encrypted configuration.
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <param name="required">When true - this setting is required and can not be empty</param>
        /// <returns>The configuration value</returns>
        public static string ReadConfig(string settingName, bool required)
        {
            string configValue = Environment.GetEnvironmentVariable($"APPSETTING_{settingName}", EnvironmentVariableTarget.Process);
            Diagnostics.EnsureArgument(!(required && string.IsNullOrEmpty(configValue)), () => required, $"Configuration item '{settingName}' is required and missing");

            Console.WriteLine($"Config: '{settingName}'='{configValue}'");
            return configValue;
        }

        /// <summary>
        /// This method reads configuration connection strings settings from app.config.
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <param name="required">When true - this setting is required and can not be empty</param>
        /// <returns>The connection string configuration value</returns>
        public static string ReadConfigConnectionString(string settingName, bool required)
        {
            string configValue = Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{settingName}", EnvironmentVariableTarget.Process);
            Diagnostics.EnsureArgument(!(required && string.IsNullOrEmpty(configValue)), () => required, $"Connection string item '{settingName}' is required and missing");
            return configValue;
        }

        /// <summary>
        /// This method reads configuration list from app.config
        /// This setting is required and will throw an exception if not exists.
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <param name="delimiter">The list delimiter</param>
        /// <returns>The configuration value, split to items according to <paramref name="delimiter"/>.</returns>
        public static IEnumerable<string> ReadConfigList(string settingName, char delimiter = ';')
        {
            var value = ReadConfig(settingName, required: true);
            var separators = new[] { delimiter };

            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// This method reads a JSON configuration item from app.config
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <returns>The configuration value, JSON deserialized to <typeparamref name="T"/>.</returns>
        /// <typeparam name="T">The type of the returned configuration item.</typeparam>
        public static T ReadConfigJson<T>(string settingName)
        {
            var value = ReadConfig(settingName, required: true);

            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}