//-----------------------------------------------------------------------
// <copyright file="FilterableCollectionTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FilterableCollectionTests
    {
        [TestMethod]
        public void WhenCreatingANewFilterableCollectionThenItIsInitializedAsExpected()
        {
            List<string> collection = new List<string>() { "Marcelo", "Willian", "Casemiro", "Messi" };
            FilterableCollection<string> filterableCollection = new FilterableCollection<string>(collection);

            CollectionAssert.AreEqual(collection, filterableCollection.OriginalCollection.ToList(), "OriginalCollection is not as expected");
            CollectionAssert.AreEqual(collection, filterableCollection.FilteredCollection.ToList(), "FilteredCollection is not as expected");
        }

        [TestMethod]
        public void WhenFilteringAFilterableCollectionThenItIsBeingFiltered()
        {
            List<string> collection = new List<string>() { "Marcelo", "Willian", "Casemiro", "Messi" };
            FilterableCollection<string> filterableCollection = new FilterableCollection<string>(collection);

            filterableCollection.Filter = str => !str.Contains('e');

            CollectionAssert.AreEqual(collection, filterableCollection.OriginalCollection.ToList(), "OriginalCollection is not as expected");
            CollectionAssert.AreEqual(new List<string>() { "Willian" }, filterableCollection.FilteredCollection.ToList(), "FilteredCollection is not as expected");
        }
    }
}
