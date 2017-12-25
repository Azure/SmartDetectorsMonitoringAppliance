//-----------------------------------------------------------------------
// <copyright file="AlertRuleStoreException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions
{
    using System;

    /// <summary>
    /// This exception is thrown when the we failed to handle against the Alert Rule store.
    /// </summary>
    public class AlertRuleStoreException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertRuleStoreException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="e">The inner exception.</param>
        public AlertRuleStoreException(string message, Exception e) : base(message, e)
        {
        }
    }
}
