//-----------------------------------------------------------------------
// <copyright file="SmartSignalRepositoryException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;

    /// <summary>
    /// Represents an exception thrown by the Smart Signal Repository.
    /// </summary>
    public class SmartSignalRepositoryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRepositoryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SmartSignalRepositoryException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRepositoryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public SmartSignalRepositoryException(string message, Exception e) : base(message, e)
        {
        }
    }
}
