namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;

    /// <summary>
    /// This exception is thrown when the we failed to handle against the Smart Signal Configuration store.
    /// </summary>
    public class SmartSignalConfigurationStoreException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalConfigurationStoreException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public SmartSignalConfigurationStoreException(string message, Exception e) : base(message, e)
        {
        }
    }
}
