//-----------------------------------------------------------------------
// <copyright file="SmartDetectorManifestTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Resources.NeutralResourcesLanguage("en")]
namespace SmartDetectorsSharedTests
{
    using System;
    using System.Text;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using ResourceType = Microsoft.Azure.Monitoring.SmartDetectors.ResourceType;

    [TestClass]
    public class SmartDetectorManifestTests
    {
        #region Happy flow tests

        [TestMethod]
        public void WhenDeserializingValidManifestThenAllValuesAreSetCorrectly()
        {
            SmartDetectorManifest manifest = DeserializeManifestFromResource(ManifestsResources.AllValues);
            Assert.AreEqual("id", manifest.Id);
            Assert.AreEqual("name", manifest.Name);
            Assert.AreEqual("description", manifest.Description);
            Assert.AreEqual(new Version(1, 0), manifest.Version);
            Assert.AreEqual("TestSmartDetectorLibrary.dll", manifest.AssemblyName);
            Assert.AreEqual("TestSmartDetectorLibrary.TestSmartDetectorWithDependency", manifest.ClassName);

            Assert.AreEqual(1, manifest.SupportedResourceTypes.Count);
            Assert.AreEqual(ResourceType.Subscription, manifest.SupportedResourceTypes[0]);

            Assert.AreEqual(1, manifest.SupportedCadencesInMinutes.Count);
            Assert.AreEqual(60, manifest.SupportedCadencesInMinutes[0]);

            Assert.AreEqual(2, manifest.ImagePaths.Count);
            Assert.AreEqual("someImage", manifest.ImagePaths[0]);
            Assert.AreEqual("anotherImage", manifest.ImagePaths[1]);

            Assert.AreEqual(2, manifest.ParametersDefinitions.Count);
            Assert.AreEqual("param1", manifest.ParametersDefinitions[0].Name);
            Assert.AreEqual(DetectorParameterType.String, manifest.ParametersDefinitions[0].Type);
            Assert.AreEqual("first parameter", manifest.ParametersDefinitions[0].DisplayName);
            Assert.AreEqual("the first parameter for the detector", manifest.ParametersDefinitions[0].Description);
            Assert.AreEqual(true, manifest.ParametersDefinitions[0].IsMandatory);
            Assert.AreEqual("param2", manifest.ParametersDefinitions[1].Name);
            Assert.AreEqual(DetectorParameterType.Integer, manifest.ParametersDefinitions[1].Type);
            Assert.AreEqual("second parameter", manifest.ParametersDefinitions[1].DisplayName);
            Assert.IsNull(manifest.ParametersDefinitions[1].Description);
            Assert.AreEqual(false, manifest.ParametersDefinitions[1].IsMandatory);
        }

        [TestMethod]
        public void WhenDeserializingValidManifestWithNoOptionalPropertiesThenAllValuesAreSetCorrectly()
        {
            SmartDetectorManifest manifest = DeserializeManifestFromResource(ManifestsResources.NoOptionalProperties);
            Assert.AreEqual("id", manifest.Id);
            Assert.AreEqual("name", manifest.Name);
            Assert.AreEqual("description", manifest.Description);
            Assert.AreEqual(new Version(1, 0), manifest.Version);
            Assert.AreEqual("TestSmartDetectorLibrary.dll", manifest.AssemblyName);
            Assert.AreEqual("TestSmartDetectorLibrary.TestSmartDetectorWithDependency", manifest.ClassName);

            Assert.AreEqual(1, manifest.SupportedResourceTypes.Count);
            Assert.AreEqual(ResourceType.Subscription, manifest.SupportedResourceTypes[0]);

            Assert.AreEqual(1, manifest.SupportedCadencesInMinutes.Count);
            Assert.AreEqual(60, manifest.SupportedCadencesInMinutes[0]);

            Assert.AreEqual(0, manifest.ImagePaths.Count);
            Assert.AreEqual(0, manifest.ParametersDefinitions.Count);
        }

        #endregion

        #region Error cases

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoIdThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithEmptyIdThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithEmptyNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoDescriptionThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoDescription);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithEmptyDescriptionThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyDescription);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoVersionThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonSerializationException))]
        public void WhenDeserializingManifestWithEmptyVersionThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonSerializationException))]
        public void WhenDeserializingManifestWithMalformedVersionThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.MalformedVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoAssemblyNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoAssemblyName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithEmptyAssemblyNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyAssemblyName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoClassNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoClassName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithEmptyClassNameThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptyClassName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoSupportedResourceTypesThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoSupportedResourceTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenDeserializingManifestWithEmptySupportedResourceTypesThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptySupportedResourceTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeserializingManifestWithNoSupportedCadencesInMinutesThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.NoSupportedCadencesInMinutes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenDeserializingManifestWithEmptySupportedCadencesInMinutesThenExceptionIsThrown()
        {
            DeserializeManifestFromResource(ManifestsResources.EmptySupportedCadencesInMinutes);
        }

        #endregion

        private static SmartDetectorManifest DeserializeManifestFromResource(byte[] resourceBytes)
        {
            return JsonConvert.DeserializeObject<SmartDetectorManifest>(Encoding.Default.GetString(resourceBytes));
        }
    }
}
