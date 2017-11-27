namespace Microsoft.SmartAlerts.Shared
{
    using Newtonsoft.Json;

    public class SignalMetadata
    {
        /// <summary>
        /// Gets the signal's id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the signal's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the signal's description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the signal's version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the name of the signal's assembly file.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the (fully qualified) name for the signal's class.
        /// </summary>
        public string ClassName { get; }

        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to SignalMetadata return false.
            var other = obj as SignalMetadata;
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
                this.ClassName.Equals(other.ClassName);
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
                return hash;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Id={this.Id}, Name={this.Name}, Description={this.Description}, Version={this.Version}";
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMetadata"/> class.
        /// </summary>
        /// <param name="id">The signal's id.</param>
        /// <param name="name">The signal's name.</param>
        /// <param name="description">The signal's description.</param>
        /// <param name="version">The signal's version</param>
        /// <param name="assemblyName">The name of the signal's assembly file.</param>
        /// <param name="className">The (fully qualified) name for the signal's class.</param>
        [JsonConstructor]
        internal SignalMetadata(string id, string name, string description, string version, string assemblyName, string className)
        {
            this.Id = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => name);
            this.Name = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => name);
            this.Description = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => description);
            this.Version = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => version);
            this.AssemblyName = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => assemblyName);
            this.AssemblyName = Diagnostics.EnsureStringNotNullOrWhiteSpace(() => className);
        }
    }
}