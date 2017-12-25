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
        private int numberOfDetectionsFound;
        private string userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="detectionsRepository">The detections repository model.</param>
        /// <param name="authenticationServices">The authentication services to use.</param>
        public MainWindowViewModel(DetectionsRepository detectionsRepository, AuthenticationServices authenticationServices)
        {
            this.NumberOfDetectionsFound = 0;
            detectionsRepository.Detections.CollectionChanged +=
                (sender, args) => { this.NumberOfDetectionsFound = args.NewItems.Count; };

            authenticationServices.UserAuthenticated += (sender, args) =>
            {
                this.UserName = args.UserInfo?.GivenName;
            };
        }

        /// <summary>
        /// Gets the number of detections found in this run.
        /// </summary>
        public int NumberOfDetectionsFound
        {
            get
            {
                return this.numberOfDetectionsFound;
            }

            private set
            {
                this.numberOfDetectionsFound = value;
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
