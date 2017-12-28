//-----------------------------------------------------------------------
// <copyright file="InvalidSmartSignalPackageException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;

    /// <summary>
    /// This exception is thrown when the smart signal package is invalid
    /// </summary>
    [Serializable]
    public class InvalidSmartSignalPackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartSignalPackageException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InvalidSmartSignalPackageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSmartSignalPackageException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public InvalidSmartSignalPackageException(string message, Exception e) : base(message, e)
        {
        }
    }
}
