//-----------------------------------------------------------------------
// <copyright file="FilterableCollection.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Represents a filterable observable collection.
    /// </summary>
    /// <typeparam name="TElement">The collection element type</typeparam>
    public class FilterableCollection<TElement> : ObservableObject
    {
        private ObservableCollection<TElement> originalCollection;

        private ObservableCollection<TElement> filteredCollection;

        private Func<TElement, bool> filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterableCollection{TElement}"/> class.
        /// </summary>
        /// <param name="originalCollection">The original collection</param>
        public FilterableCollection(IEnumerable<TElement> originalCollection)
        {
            this.OriginalCollection = new ObservableCollection<TElement>(originalCollection);
            this.FilteredCollection = new ObservableCollection<TElement>();
            this.Filter = m => true;
        }

        /// <summary>
        /// Gets the original collection.
        /// </summary>
        public ObservableCollection<TElement> OriginalCollection
        {
            get => this.originalCollection;

            private set
            {
                this.originalCollection = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the filtered collection.
        /// </summary>
        public ObservableCollection<TElement> FilteredCollection
        {
            get
            {
                return this.filteredCollection;
            }

            private set
            {
                this.filteredCollection = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filter. When setting a new filter it will filter the original collection accordingly.
        /// </summary>
        public Func<TElement, bool> Filter
        {
            get
            {
                return this.filter;
            }

            set
            {
                this.filter = value;

                this.FilteredCollection.Clear();
                foreach (TElement element in this.OriginalCollection.Where(this.filter))
                {
                    this.FilteredCollection.Add(element);
                }

                this.OnPropertyChanged();
            }
        }
    }
}
