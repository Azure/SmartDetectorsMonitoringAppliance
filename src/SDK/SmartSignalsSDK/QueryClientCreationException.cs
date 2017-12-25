//-----------------------------------------------------------------------
// <copyright file="QueryClientCreationException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where a query client could not be created by the <see cref="IAnalysisServicesFactory"/>.
    /// A typical scenario is if the resources that needs to be analyzed do not match the query client type.
    /// </summary>
    [Serializable]
    public class QueryClientCreationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryClientCreationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public QueryClientCreationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryClientCreationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected QueryClientCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}