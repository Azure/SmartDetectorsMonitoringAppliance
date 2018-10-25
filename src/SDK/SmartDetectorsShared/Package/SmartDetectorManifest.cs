//-----------------------------------------------------------------------
// <copyright file="SmartDetectorManifest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Package
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represents the manifest of a Smart Detector, stored in the Smart Detectors repository
    /// </summary>
    public sealed class SmartDetectorManifest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorManifest"/> class.
        /// </summary>
        /// <param name="id">The Smart Detector's id.</param>
        /// <param name="name">The Smart Detector's name.</param>
        /// <param name="description">The Smart Detector's description.</param>
        /// <param name="version">The Smart Detector's version</param>
        /// <param name="assemblyName">The name of the Smart Detector's assembly file.</param>
        /// <param name="className">The (fully qualified) name for the Smart Detector's class.</param>
        /// <param name="supportedResourceTypes">The types of resources that this Smart Detector supports</param>
        /// <param name="supportedCadencesInMinutes">The cadences that this Smart Detector can be executed</param>
        /// <param name="imagePaths">The image paths in the package</param>
        public SmartDetectorManifest(
            string id,
            string name,
            string description,
            Version version,
            string assemblyName,
            string className,
            IReadOnlyList<ResourceType> supportedResourceTypes,
            IReadOnlyList<int> supportedCadencesInMinutes,
            IReadOnlyList<string> imagePaths)
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
                throw new ArgumentException("A Smart Detector must support at least one resource type", nameof(supportedResourceTypes));
            }

            if (supportedCadencesInMinutes == null)
            {
                throw new ArgumentNullException(nameof(supportedCadencesInMinutes));
            }

            if (supportedCadencesInMinutes.Count == 0)
            {
                throw new ArgumentException("A Smart Detector must support at least one cadence", nameof(supportedCadencesInMinutes));
            }

            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Version = version;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.SupportedResourceTypes = supportedResourceTypes;
            this.SupportedCadencesInMinutes = supportedCadencesInMinutes;
            this.ImagePaths = imagePaths ?? new List<string>();
        }

        /// <summary>
        /// Gets the Smart Detector's id.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; }

        /// <summary>
        /// Gets the Smart Detector's name.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; }

        /// <summary>
        /// Gets the Smart Detector's description.
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; }

        /// <summary>
        /// Gets the Smart Detector's version.
        /// </summary>
        [JsonProperty("version", Required = Required.Always)]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; }

        /// <summary>
        /// Gets the name of the Smart Detector's assembly file.
        /// </summary>
        [JsonProperty("assemblyName", Required = Required.Always)]
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the (fully qualified) name for the Smart Detector's class.
        /// </summary>
        [JsonProperty("className", Required = Required.Always)]
        public string ClassName { get; }

        /// <summary>
        /// Gets the types of resources that this Smart Detector supports
        /// </summary>
        [JsonProperty("supportedResourceTypes", Required = Required.Always)]
        public IReadOnlyList<ResourceType> SupportedResourceTypes { get; }

        /// <summary>
        /// Gets the Smart Detector supported cadences in minutes.
        /// </summary>
        [JsonProperty("supportedCadencesInMinutes", Required = Required.Always)]
        public IReadOnlyList<int> SupportedCadencesInMinutes { get; }

        /// <summary>
        /// Gets the Smart Detector images paths.
        /// </summary>
        [JsonProperty("imagePaths")]
        public IReadOnlyList<string> ImagePaths { get; }

        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to SmartDetectorManifest return false.
            var other = obj as SmartDetectorManifest;
            if (other == null)
            {
                return false;
            }

            // And validate the properties (constructor ensures those are never null)
            return
                this.Id.Equals(other.Id, StringComparison.InvariantCulture) &&
                this.Name.Equals(other.Name, StringComparison.InvariantCulture) &&
                this.Description.Equals(other.Description, StringComparison.InvariantCulture) &&
                this.Version.Equals(other.Version) &&
                this.AssemblyName.Equals(other.AssemblyName, StringComparison.InvariantCulture) &&
                this.ClassName.Equals(other.ClassName, StringComparison.InvariantCulture) &&
                this.SupportedResourceTypes.SequenceEqual(other.SupportedResourceTypes) &&
                this.SupportedCadencesInMinutes.SequenceEqual(other.SupportedCadencesInMinutes) &&
                this.ImagePaths.SequenceEqual(other.ImagePaths);
        }

        /// <summary>
        /// Returns the hash code for the current object.
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

                if (this.ImagePaths != null)
                {
                    foreach (string imagePath in this.ImagePaths)
                    {
                        hash = (31 * hash) + imagePath.GetHashCode();
                    }
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
            return $"Id={this.Id}, Name={this.Name}, Description={this.Description}, Version={this.Version}, SupportedResourceTypes={string.Join("|", this.SupportedResourceTypes)}, SupportedCadencesInMinutes={string.Join("|", this.SupportedCadencesInMinutes)}, ImagePaths={string.Join("|", this.ImagePaths)}";
        }

        #endregion
    }
}