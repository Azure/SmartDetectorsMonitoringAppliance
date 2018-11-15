//-----------------------------------------------------------------------
// <copyright file="SmartDetectorPackageTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SmartDetectorPackageTests
    {
        #region Happy flow tests

        [TestMethod]
        public void WhenCreatingPackageWithValidValuesThenPackageIsCreated()
        {
            var package = new SmartDetectorPackage(GetDefaultPackageContent());
        }

        [TestMethod]
        public void WhenCreatingPackageWithValidValuesAndDllSuffixForTheAssemblyThenPackageIsCreated()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            content["TestSmartDetectorLibrary.dll"] = new byte[] { 0 };
            var package = new SmartDetectorPackage(content);
        }

        [TestMethod]
        public void WhenCreatingPackageWithValidValuesAndExeSuffixForTheAssemblyThenPackageIsCreated()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            content["TestSmartDetectorLibrary.exe"] = new byte[] { 0 };
            var package = new SmartDetectorPackage(content);
        }

        #endregion

        #region Error cases

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingPackageWithNullContentThenExceptionIsThrown()
        {
            var package = new SmartDetectorPackage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenCreatingPackageWithEmptyContentThenExceptionIsThrown()
        {
            var package = new SmartDetectorPackage(new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>()));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingAssemblyThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            var package = new SmartDetectorPackage(content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingImageThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("anotherImage");
            var package = new SmartDetectorPackage(content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingParameterNameThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content["manifest.json"] = ManifestsResources.NoParameterName;
            var package = new SmartDetectorPackage(content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingParameterDisplayNameThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content["manifest.json"] = ManifestsResources.NoParameterDisplayName;
            var package = new SmartDetectorPackage(content);
        }

        [TestMethod]
        public void WhenCreatingPackageWithInvalidManifestThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();

            // This checks for NullArgumentException from the manifest creation
            content["manifest.json"] = ManifestsResources.NoId;
            Assert.ThrowsException<InvalidSmartDetectorPackageException>(() => new SmartDetectorPackage(content));

            // This checks for ArgumentException from the manifest creation
            content["manifest.json"] = ManifestsResources.EmptySupportedResourceTypes;
            Assert.ThrowsException<InvalidSmartDetectorPackageException>(() => new SmartDetectorPackage(content));

            // This checks for JsonException from the manifest creation
            content["manifest.json"] = ManifestsResources.MalformedVersion;
            Assert.ThrowsException<InvalidSmartDetectorPackageException>(() => new SmartDetectorPackage(content));
        }

        #endregion

        private static Dictionary<string, byte[]> GetDefaultPackageContent()
        {
            return new Dictionary<string, byte[]>
            {
                ["manifest.json"] = ManifestsResources.AllValues,
                ["TestSmartDetectorLibrary"] = new byte[] { 0 },
                ["someImage"] = new byte[] { 0 },
                ["anotherImage"] = new byte[] { 0 },
            };
        }
    }
}
