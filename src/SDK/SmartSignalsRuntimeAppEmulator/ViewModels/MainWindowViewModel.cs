//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;

    /// <summary>
    /// The view model class for the <see cref="MainWindow"/> control.
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        private int numberOfResultsFound;
        private string userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="signalsResultsRepository">The signal results repository model.</param>
        /// <param name="authenticationServices">The authentication services to use.</param>
        public MainWindowViewModel(SignalsResultsRepository signalsResultsRepository, AuthenticationServices authenticationServices)
        {
            this.NumberOfResultsFound = 0;
            signalsResultsRepository.Results.CollectionChanged +=
                (sender, args) => { this.NumberOfResultsFound = args.NewItems.Count; };

            authenticationServices.UserAuthenticated += (sender, args) =>
            {
                this.UserName = args.UserInfo?.GivenName;
            };
        }

        /// <summary>
        /// Gets the number of results found in this run.
        /// </summary>
        public int NumberOfResultsFound
        {
            get
            {
                return this.numberOfResultsFound;
            }

            private set
            {
                this.numberOfResultsFound = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the name of the signed in user.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userName;
            }

            private set
            {
                this.userName = value;
                this.OnPropertyChanged();
            }
        }
    }
}
