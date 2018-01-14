//-----------------------------------------------------------------------
// <copyright file="SmartSignalManifest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Package
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represents the manifest of a Smart Signal, stored in the smart signals repository
    /// </summary>
    public sealed class SmartSignalManifest
    {    
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalManifest"/> class.
        /// </summary>
        /// <param name="id">The signal's id.</param>
        /// <param name="name">The signal's name.</param>
        /// <param name="description">The signal's description.</param>
        /// <param name="version">The signal's version</param>
        /// <param name="assemblyName">The name of the signal's assembly file.</param>
        /// <param name="className">The (fully qualified) name for the signal's class.</param>
        /// <param name="supportedResourceTypes">The types of resources that this signal supports</param>
        /// <param name="supportedCadencesInMinutes">The cadences that this signal can be executed</param>
        public SmartSignalManifest(string id, string name, string description, Version version, string assemblyName, string className, IReadOnlyList<ResourceType> supportedResourceTypes, IReadOnlyList<int> supportedCadencesInMinutes)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (supportedResourceTypes == null)
            {
                throw new ArgumentNullException(nameof(supportedResourceTypes));
            }

            if (supportedResourceTypes.Count == 0)
            {                
                throw new ArgumentException("A signal must support at least one resource type", nameof(supportedResourceTypes));
            }

            if (supportedCadencesInMinutes == null)
            {
                throw new ArgumentNullException(nameof(supportedCadencesInMinutes));
            }

            if (supportedCadencesInMinutes.Count == 0)
            {
                throw new ArgumentException("A signal must support at least one cadence", nameof(supportedCadencesInMinutes));
            }

            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Version = version;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.SupportedResourceTypes = supportedResourceTypes;
            this.SupportedCadencesInMinutes = supportedCadencesInMinutes;
        }

        /// <summary>
        /// Gets the signal's id.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; }

        /// <summary>
        /// Gets the signal's name.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; }

        /// <summary>
        /// Gets the signal's description.
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; }

        /// <summary>
        /// Gets the signal's version.
        /// </summary>
        [JsonProperty("version", Required = Required.Always)]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; }

        /// <summary>
        /// Gets the name of the signal's assembly file.
        /// </summary>
        [JsonProperty("assemblyName", Required = Required.Always)]
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the (fully qualified) name for the signal's class.
        /// </summary>
        [JsonProperty("className", Required = Required.Always)]
        public string ClassName { get; }

        /// <summary>
        /// Gets the types of resources that this signal supports
        /// </summary>
        [JsonProperty("supportedResourceTypes", Required = Required.Always)]
        public IReadOnlyList<ResourceType> SupportedResourceTypes { get; }

        /// <summary>
        /// Gets the signal supported cadences in minutes.
        /// </summary>
        [JsonProperty("supportedCadencesInMinutes", Required = Required.Always)]
        public IReadOnlyList<int> SupportedCadencesInMinutes { get; }

        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to SmartSignalManifest return false.
            var other = obj as SmartSignalManifest;
            if (other == null)
            {
                return false;
            }

            // And validate the properties (constructor ensures those are never null)
            return
                this.Id.Equals(other.Id) &&
                this.Name.Equals(other.Name) &&
                this.Description.Equals(other.Description) &&
                this.Version.Equals(other.Version) &&
                this.AssemblyName.Equals(other.AssemblyName) &&
                this.ClassName.Equals(other.ClassName) &&
                this.SupportedResourceTypes.SequenceEqual(other.SupportedResourceTypes) &&
                this.SupportedCadencesInMinutes.SequenceEqual(other.SupportedCadencesInMinutes);
        }

        /// <summary>
        /// Returns the hash code for the current object.. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = this.Id.GetHashCode();
                hash = (31 * hash) + this.Name.GetHashCode();
                hash = (31 * hash) + this.Description.GetHashCode();
                hash = (31 * hash) + this.Version.GetHashCode();
                hash = (31 * hash) + this.AssemblyName.GetHashCode();

                foreach (ResourceType resourceType in this.SupportedResourceTypes)
                {
                    hash = (31 * hash) + resourceType.GetHashCode();
                }

                foreach (int supportedCadence in this.SupportedCadencesInMinutes)
                {
                    hash = (31 * hash) + supportedCadence.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Id={this.Id}, Name={this.Name}, Description={this.Description}, Version={this.Version}, SupportedResourceTypes={string.Join("|", this.SupportedResourceTypes)}, SupportedCadencesInMinutes={string.Join("|", this.SupportedCadencesInMinutes)}";
        }

        #endregion
    }
}