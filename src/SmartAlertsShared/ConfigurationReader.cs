// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//         Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace Microsoft.Azure.Monitoring.SmartAlerts.Shared
{
    using System;
    using System.Collections.Generic;
    using ManagementServices;
    using Newtonsoft.Json;

    public static class ConfigurationReader
    {
        /// <summary>
        /// This method is to ensure AI Configuration Encryption decrypt providers are installed
        /// </summary>
        static ConfigurationReader()
        {
            AppConfigProvider.EnsureInstalled();

            // The CMS decrypt provider is used do decrypt the encrypted configuration
            CmsCryptoProvider.EnsureInstalled();
        }

        /// <summary>
        /// This method reads configuration settings from app.config.
        /// This should be used for Non encrypted configuration.
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <param name="required">When true - this setting is required and can not be empty</param>
        /// <returns></returns>
        public static string ReadConfig(string settingName, bool required)
        {
            string configValue = Config.GetValue(settingName, required);
            Console.WriteLine($"Config: '{settingName}'='{configValue}'");
            return configValue;
        }

        /// <summary>
        /// This method reads configuration settings from app.config and returns the decrypted value
        /// </summary>
        /// <param name="settingName">The encrypted property name to read from app.config</param>
        /// <param name="required">When true - this setting is required and can not be empty</param>
        /// <returns></returns>
        public static string ReadAndDecryptConfig(string settingName, bool required)
        {
            string configValue = Config.GetDecryptedValue(settingName, required);
            string displayValue = CreateHiddenValue(configValue);
            Console.WriteLine($"Decrypted config: '{settingName}'='{displayValue}'");

            return configValue;
        }

        /// <summary>
        /// This method reads configuration connection string template and encrypted key from app.config, 
        /// decrypts the key and returns the connection string with the decrypted key
        /// e.g.
        ///  <add key="ServiceBusKeyEncrypted" value="EcryptedValue" />
        ///  <add key="ServiceBusConnectionStringTemplate" value="Endpoint=sb://url;SharedAccessKeyName=name;SharedAccessKey={KeyPlaceholder}" />
        /// Using this method to read the ServiceBusConnectionStringTemplate and ServiceBusKeyEncrypted, will decrypt the key and put it in the right place.
        /// </summary>
        /// <param name="connectionStringTemplateSettingName">The connection string template property to read from app.config</param>
        /// <param name="encryptedKeySettingName">The encrypted property name to read from app.config</param>
        /// <returns></returns>
        public static string ReadAndDecryptConfigConnectionString(string connectionStringTemplateSettingName, string encryptedKeySettingName)
        {
            string connectionStringValue = Config.GetValue(connectionStringTemplateSettingName, true);
            string keyDecryptedValue = Config.GetDecryptedValue(encryptedKeySettingName, true);
            string configValue = connectionStringValue.Replace("{KeyPlaceholder}", keyDecryptedValue);

            string displayValue = connectionStringValue.Replace("{KeyPlaceholder}", CreateHiddenValue(keyDecryptedValue));
            Console.WriteLine($"Decrypted connection string config: '{connectionStringTemplateSettingName}'='{displayValue}'");

            return configValue;
        }

        /// <summary>
        /// This method reads configuration list from app.config
        /// Configuration settings should be decrypted in this point since we use Secrets Tokenization
        /// This setting is required and will throw an exception if not exists.
        /// </summary>
        /// <param name="settingName">The property name to read from app.config</param>
        /// <param name="delimiter">The list delimiter</param>
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
        public static T ReadConfigJson<T>(string settingName)
        {
            var value = ReadConfig(settingName, required: true);

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// This method reads an encrypted JSON string from app.config and returns the decrypted JSON object
        /// </summary>
        /// <param name="settingName">The encrypted property name to read from app.config</param>
        public static T ReadAndDecryptConfigJson<T>(string settingName)
        {
            var value = ReadAndDecryptConfig(settingName, required: true);

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Helper utility to obscure secret data, and still leave it identifiable.
        /// Used to avoid tracing out secret data.
        /// example: 'My$Password' is returned as 'M*********d'
        /// </summary>
        /// <param name="value">string value to obscure</param>
        /// <returns></returns>
        private static string CreateHiddenValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= 2)
            {
                return value;
            }

            string hidden = value[0] + new string('*', value.Length - 2) + value.Substring(value.Length - 1);
            return hidden;
        }
    }
}