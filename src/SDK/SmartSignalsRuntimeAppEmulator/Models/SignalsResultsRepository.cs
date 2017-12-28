//-----------------------------------------------------------------------
// <copyright file="SignalsResultsRepository.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Signal results repository model. Holds all Smart Signal results created in the current run.
    /// </summary>
    public class SignalsResultsRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalsResultsRepository"/> class.
        /// </summary>
        public SignalsResultsRepository()
        {
            this.Results = new ObservableCollection<SmartSignalResult>();
        }

        /// <summary>
        /// Gets the collection of result in the repository.
        /// </summary>
        public ObservableCollection<SmartSignalResult> Results { get; }
    }
}
