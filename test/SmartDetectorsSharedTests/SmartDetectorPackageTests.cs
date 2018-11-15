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
            var package = new SmartDetectorPackage(GetDefaultManifest(), GetDefaultPackageContent());
        }

        [TestMethod]
        public void WhenCreatingPackageWithValidValuesAndDllSuffixForTheAssemblyThenPackageIsCreated()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            content["TestSmartDetectorLibrary.dll"] = new byte[] { 0 };
            var package = new SmartDetectorPackage(GetDefaultManifest(), content);
        }

        [TestMethod]
        public void WhenCreatingPackageWithValidValuesAndExeSuffixForTheAssemblyThenPackageIsCreated()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            content["TestSmartDetectorLibrary.exe"] = new byte[] { 0 };
            var package = new SmartDetectorPackage(GetDefaultManifest(), content);
        }

        #endregion

        #region Error cases

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingPackageWithNullManifestThenExceptionIsThrown()
        {
            var package = new SmartDetectorPackage(null, GetDefaultPackageContent());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreatingPackageWithNullContentThenExceptionIsThrown()
        {
            var package = new SmartDetectorPackage(GetDefaultManifest(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenCreatingPackageWithEmptyContentThenExceptionIsThrown()
        {
            var package = new SmartDetectorPackage(GetDefaultManifest(), new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>()));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingAssemblyThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("TestSmartDetectorLibrary");
            var package = new SmartDetectorPackage(GetDefaultManifest(), content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingImageThenExceptionIsThrown()
        {
            Dictionary<string, byte[]> content = GetDefaultPackageContent();
            content.Remove("anotherImage");
            var package = new SmartDetectorPackage(GetDefaultManifest(), content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingParameterNameThenExceptionIsThrown()
        {
            SmartDetectorManifest manifest = GetDefaultManifest();
            manifest.ParametersDefinitions[1].Name = string.Empty;
            var package = new SmartDetectorPackage(manifest, GetDefaultPackageContent());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSmartDetectorPackageException))]
        public void WhenCreatingPackageWithMissingParameterDisplayNameThenExceptionIsThrown()
        {
            SmartDetectorManifest manifest = GetDefaultManifest();
            manifest.ParametersDefinitions[1].DisplayName = string.Empty;
            var package = new SmartDetectorPackage(manifest, GetDefaultPackageContent());
        }

        #endregion

        private static Dictionary<string, byte[]> GetDefaultPackageContent()
        {
            return new Dictionary<string, byte[]>
            {
                { "Manifest.json", ManifestsResources.AllValues },
                { "TestSmartDetectorLibrary", new byte[] { 0 } },
                { "someImage", new byte[] { 0 } },
                { "anotherImage", new byte[] { 0 } },
            };
        }

        private static SmartDetectorManifest GetDefaultManifest()
        {
            return JsonConvert.DeserializeObject<SmartDetectorManifest>(Encoding.Default.GetString(ManifestsResources.AllValues));
        }
    }
}
