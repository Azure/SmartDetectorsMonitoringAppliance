//-----------------------------------------------------------------------
// <copyright file="ObservableObject.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A base class for any observable object. Contains convenient implementation of the <see cref="INotifyPropertyChanged"/>
    /// interface, for inheriting classes to use when changing observable properties.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper method for notifying that a property has changed.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that has changed. Since we're using <see cref="CallerMemberNameAttribute"/>, calling
        /// this method from inside a property setter method does not require specifying the property name.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
